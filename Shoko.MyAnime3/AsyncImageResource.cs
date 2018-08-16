﻿#region GNU license
// MP-TVSeries - Plugin for Mediaportal
// http://www.team-mediaportal.com
// Copyright (C) 2006-2007
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//
//+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
//+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
#endregion


using System;
using System.Threading;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Reflection;
using MediaPortal.GUI.Library;
using System.IO;
using Shoko.MyAnime3;


namespace Cornerstone.MP
{
    public delegate void AsyncImageLoadComplete(AsyncImageResource image);

    public class AsyncImageResource
    {
        private Object loadingLock = new Object();
        private int pendingToken = 0;
        private int threadsWaiting = 0;
        private bool warned = false;


        /// <summary>
        /// This event is triggered when a new image file has been successfully loaded
        /// into memory.
        /// </summary>
        public event AsyncImageLoadComplete ImageLoadingComplete;

        /// <summary>
        /// True if this resources will actively load into memory when assigned a file.
        /// </summary>
        public bool Active
        {
            get
            {
                return _active;
            }

            set
            {
                if (_active == value)
                    return;

                _active = value;

                Thread newThread = new Thread(new ThreadStart(activeWorker));
                newThread.Name = "AsyncImageResource.activeWorker";
                newThread.Start();
            }
        }
        private bool _active = true;

        /// <summary>
        /// If multiple changes to the Filename property are made in rapid succession, this delay
        /// will be used to prevent unecessary loading operations. Most useful for large images that
        /// take a non-trivial amount of time to load from memory.
        /// </summary>
        public int Delay
        {
            get { return _delay; }
            set { _delay = value; }
        }
        private int _delay = 250;

        private void activeWorker()
        {
            lock (loadingLock)
            {
                if (_active)
                {
                    // load the resource
                    _identifier = loadResourceSafe(_filename);

                    // notify any listeners a resource has been loaded
                    if (ImageLoadingComplete != null)
                        ImageLoadingComplete(this);
                }
                else
                {
                    unloadResource(_filename);
                    _identifier = null;
                }
            }
        }


        /// <summary>
        /// This MediaPortal property will automatically be set with the renderable identifier
        /// once the resource has been loaded. Appropriate for a texture field of a GUIImage 
        /// control.
        /// </summary>
        public string Property
        {
            get { return _property; }
            set
            {
                _property = value;

                writeProperty();
            }
        }
        private string _property = null;

        private void writeProperty()
        {
            if (_active && _property != null && _identifier != null)
                GUIPropertyManager.SetProperty(_property, _identifier);
            else
                if (_property != null)
                GUIPropertyManager.SetProperty(_property, "-");
        }


        /// <summary>
        /// The identifier used by the MediaPortal GUITextureManager to identify this resource.
        /// This changes when a new file has been assigned, if you need to know when this changes
        /// use the ImageLoadingComplete event.
        /// </summary>
        public string Identifier
        {
            get { return _identifier; }
        }
        string _identifier = null;


        /// <summary>
        /// The filename of the image backing this resource. Reassign to change textures.
        /// </summary>
        public string Filename
        {
            get
            {
                return _filename;
            }

            set
            {
                if (value == null)
                    value = " ";

                Thread newThread = new Thread(new ParameterizedThreadStart(setFilenameWorker));
                newThread.Name = "AsyncImageResource.setFilenameWorker";
                newThread.Start(value);
            }
        }
        string _filename = null;

        // Unloads the previous file and sets a new filename. 
        private void setFilenameWorker(object newFilenameObj)
        {
            int localToken = ++pendingToken;
            string oldFilename = _filename;

            // check if another thread has locked for loading
            bool loading = Monitor.TryEnter(loadingLock);
            if (loading) Monitor.Exit(loadingLock);

            // if a loading action is in progress or another thread is waiting, we wait too
            if (loading || threadsWaiting > 0)
            {
                threadsWaiting++;
                for (int i = 0; i < 5; i++)
                {
                    Thread.Sleep(_delay / 5);
                    if (localToken < pendingToken)
                        return;
                }
                threadsWaiting--;
            }

            lock (loadingLock)
            {
                if (localToken < pendingToken)
                    return;

                // type cast and clean our filename
                string newFilename = (string)newFilenameObj;
                if (newFilename != null && newFilename.Trim().Length == 0)
                    newFilename = null;
                else if (newFilename != null)
                    newFilename = newFilename.Trim();

                // if we are not active we should nto be assigning a filename
                if (!Active) newFilename = null;

                // if there is no change, quit
                if (_filename != null && _filename.Equals(newFilename))
                {
                    if (ImageLoadingComplete != null)
                        ImageLoadingComplete(this);

                    return;
                }

                string newIdentifier = loadResourceSafe(newFilename);

                // check if we have a new loading action pending, if so just quit
                if (localToken < pendingToken)
                {
                    unloadResource(newIdentifier);
                    return;
                }

                // update MediaPortal about the image change
                _identifier = newIdentifier;
                _filename = newFilename;
                writeProperty();

                // notify any listeners a resource has been loaded
                if (ImageLoadingComplete != null)
                    ImageLoadingComplete(this);
            }

            // wait a few seconds in case we want to quickly reload the previous resource
            // if it's not reassigned, unload from memory.
            Thread.Sleep(5000);
            lock (loadingLock)
            {
                if (_filename != oldFilename)
                    unloadResource(oldFilename);
            }
        }


        /// <summary>
        /// Loads the given file into memory and registers it with MediaPortal.
        /// </summary>
        /// <param name="filename">The image file to be loaded.</param>
        private bool loadResource(string filename)
        {
            if (!_active || filename == null || !File.Exists(filename))
                return false;

            try
            {
                if (GUITextureManager.Load(filename, 0, 0, 0, true) > 0)
                    return true;
            }
            catch { }

            return false;
        }

        private string loadResourceSafe(string filename)
        {
            if (filename == null || filename.Trim().Length == 0)
                return null;

            // try to load with new persistent load feature
            try
            {
                if (loadResource(filename))
                    return filename;
            }
            catch (MissingMethodException)
            {
                if (!warned)
                {
                    BaseConfig.MyAnimeLog.Write("AsyncImageResource: Cannot preform asynchronous loading with this version of MediaPortal. Please upgrade for improved performance.");
                    warned = true;
                }
            }

            // if not available load image ourselves and pass to MediaPortal. Much slower but this still
            // gives us asynchronous loading. 
            Image image = LoadImageFastFromFile(filename);
            //MPTVSeriesLog.WriteMultiLine("AsyncImageResource LoadFromMemory - " + Environment.StackTrace, MPTVSeriesLog.LogLevel.Debug);
            if (GUITextureManager.LoadFromMemory(image, getIdentifier(filename), 0, 0, 0) > 0)
            {
                return getIdentifier(filename);
            }

            return null;
        }

        private string getIdentifier(string filename)
        {
            return "[Anime3:" + filename.GetHashCode() + "]";
        }

        /// <summary>
        /// If previously loaded, unloads the resource from memory and removes it 
        /// from the MediaPortal GUITextureManager.
        /// </summary>
        private void unloadResource(string filename)
        {

            if (filename == null)
                return;

            // double duty since we dont know if we loaded via new fast way or old
            // slow way
            GUITextureManager.ReleaseTexture(getIdentifier(filename));
            GUITextureManager.ReleaseTexture(filename);
        }


        [DllImport("gdiplus.dll", CharSet = CharSet.Unicode)]
        private static extern int GdipLoadImageFromFile(string filename, out IntPtr image);

        // Loads an Image from a File by invoking GDI Plus instead of using build-in 
        // .NET methods, or falls back to Image.FromFile. GDI Plus should be faster.
        public static Image LoadImageFastFromFile(string filename)
        {
            IntPtr imagePtr = IntPtr.Zero;
            Image image = null;

            try
            {
                if (GdipLoadImageFromFile(filename, out imagePtr) != 0)
                {
                    BaseConfig.MyAnimeLog.Write("AsyncImageResource: gdiplus.dll method failed. Will degrade performance.");
                    image = Image.FromFile(filename);
                }

                else
                    image = (Image)typeof(Bitmap).InvokeMember("FromGDIplus", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod, null, null, new object[] { imagePtr });
            }
            catch (Exception)
            {
                BaseConfig.MyAnimeLog.Write("AsyncImageResource: Failed to load image from {0}", filename);
                image = null;
            }

            return image;

        }

    }
}


/*

//using NLog;
using System;
using System.Threading;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Reflection;
using MediaPortal.GUI.Library;
using System.IO;
using Shoko.MyAnime3;

namespace Cornerstone.MP {
    public delegate void AsyncImageLoadComplete(AsyncImageResource image);
    
    public class AsyncImageResource {
        //private static Logger logger = LogManager.GetCurrentClassLogger();
        
        private Object loadingLock = new Object();
        private int pendingToken = 0;
        private int threadsWaiting = 0;
        private bool warned = false;


        /// <summary>
        /// This event is triggered when a new image file has been successfully loaded
        /// into memory.
        /// </summary>
        public event AsyncImageLoadComplete ImageLoadingComplete;

        /// <summary>
        /// True if this resources will actively load into memory when assigned a file.
        /// </summary>
        public bool Active {
            get {
                return _active;
            }

            set {
                if (_active == value)
                    return;

                _active = value;

                Thread newThread = new Thread(new ThreadStart(activeWorker));
                newThread.Name = "AsyncImageResource.activeWorker";
                newThread.Start();
            }
        }
        private bool _active = true;

        /// <summary>
        /// If multiple changes to the Filename property are made in rapid succession, this delay
        /// will be used to prevent uneccisary loading operations. Most useful for large images that
        /// take a non-trivial amount of time to load from memory.
        /// </summary>
        public int Delay {
            get { return _delay; }
            set { _delay = value; }
        } private int _delay = 250;

        private void activeWorker() {
            lock (loadingLock) {
                if (_active) {
                    // load the resource
                    _identifier = loadResourceSafe(_filename);

                    // notify any listeners a resource has been loaded
                    if (ImageLoadingComplete != null)
                        ImageLoadingComplete(this);
                }
                else {
                    unloadResource(_filename);
                    _identifier = null;
                }
            }
        }
        

        /// <summary>
        /// This MediaPortal property will automatically be set with the renderable identifier
        /// once the resource has been loaded. Appropriate for a texture field of a GUIImage 
        /// control.
        /// </summary>
        public string Property {
            get { return _property; }
            set { 
                _property = value;

                writeProperty();
            }
        }
        private string _property = null;

        private void writeProperty()
        {
            BaseConfig.MyAnimeLog.Write("Try Write Prop: " + _property + " to " + _identifier);
            if (_active && _property != null && _identifier != null)
            {
                BaseConfig.MyAnimeLog.Write("Write Prop: " + _property + " to " + _identifier);
                GUIPropertyManager.SetProperty(_property, _identifier);
            }
            else if (_property != null)
                GUIPropertyManager.SetProperty(_property, "-");
        }


        /// <summary>
        /// The identifier used by the MediaPortal GUITextureManager to identify this resource.
        /// This changes when a new file has been assigned, if you need to know when this changes
        /// use the ImageLoadingComplete event.
        /// </summary>
        public string Identifier {
            get { return _identifier; }
        } 
        string _identifier = null;


        /// <summary>
        /// The filename of the image backing this resource. Reassign to change textures.
        /// </summary>
        public string Filename {
            get {
                return _filename;
            }

            set {
                Thread newThread = new Thread(new ParameterizedThreadStart(setFilenameWorker));
                newThread.Name = "AsyncImageResource.setFilenameWorker";
                newThread.Start(value);
            }
        }
        string _filename = null;

        // Unloads the previous file and sets a new filename. 
        private void setFilenameWorker(object newFilenameObj) {
            int localToken = ++pendingToken;
            string oldFilename = _filename;

            // check if another thread has locked for loading
            bool loading = Monitor.TryEnter(loadingLock);
            if (loading) Monitor.Exit(loadingLock);

            // if a loading action is in progress or another thread is waiting, we wait too
            if (loading || threadsWaiting > 0) {
                threadsWaiting++;
                for (int i = 0; i < 5; i++) {
                    Thread.Sleep(_delay / 5);
                    if (localToken < pendingToken)
                        return;
                }
                threadsWaiting--;
            }

            lock (loadingLock) {
                if (localToken < pendingToken) 
                    return;

                // type cast and clean our filename
                string newFilename = (string)newFilenameObj;
                if (newFilename != null && newFilename.Trim().Length == 0)
                    newFilename = null;
                else if (newFilename != null)
                    newFilename = newFilename.Trim();

                // if we are not active we should nto be assigning a filename
                if (!Active) newFilename = null;

                // if there is no change, quit
                if (_filename != null && _filename.Equals(newFilename)) {
                    if (ImageLoadingComplete != null)
                        ImageLoadingComplete(this);

                    return;
                }

                string newIdentifier = loadResourceSafe(newFilename);

                // check if we have a new loading action pending, if so just quit
                if (localToken < pendingToken) {
                    unloadResource(newIdentifier);
                    return;
                }

                // update MediaPortal about the image change
                _identifier = newIdentifier;
                _filename = newFilename;
                writeProperty();

                // notify any listeners a resource has been loaded
                if (ImageLoadingComplete != null)
                    ImageLoadingComplete(this);
            }

            // wait a few seconds in case we want to quickly reload the previous resource
            // if it's not reassigned, unload from memory.
            Thread.Sleep(5000);
            lock (loadingLock) {
                if (_filename != oldFilename)
                    unloadResource(oldFilename);
            }
        }


        /// <summary>
        /// Loads the given file into memory and registers it with MediaPortal.
        /// </summary>
        /// <param name="filename">The image file to be loaded.</param>
        private bool loadResource(string filename) {
            if (!_active || filename == null || !File.Exists(filename))
                return false;

            try
            {
                if (GUITextureManager.Load(filename, 0, 0, 0, true) > 0)
                    return true;
            }
            catch { }
           
            return false;
        }

        private string loadResourceSafe(string filename) {
            if (filename == null || filename.Trim().Length == 0)
                return null;
            
            // try to load with new persistent load feature
            try {
                if (loadResource(filename))
                    return filename;
            }
            catch (MissingMethodException) {
                if (!warned) {
                    //logger.Warn("Cannot preform asynchronous loading with this version of MediaPortal. Please upgrade for improved performance.");
                    warned = true;
                }
            }

            // if not available load image ourselves and pass to MediaPortal. Much slower but this still
            // gives us asynchronous loading. 
            Image image = LoadImageFastFromFile(filename);
            if (GUITextureManager.LoadFromMemory(image, getIdentifier(filename), 0, 0, 0) > 0) {
                return getIdentifier(filename);
            }

            return null;
        }

        private string getIdentifier(string filename) {
            return "[TVSeries:" + filename.GetHashCode() + "]";
        }

        /// <summary>
        /// If previously loaded, unloads the resource from memory and removes it 
        /// from the MediaPortal GUITextureManager.
        /// </summary>
        private void unloadResource(string filename) {

            if (filename == null)
                return;

            // double duty since we dont know if we loaded via new fast way or old
            // slow way
            GUITextureManager.ReleaseTexture(getIdentifier(filename));
            GUITextureManager.ReleaseTexture(filename);
        }


        [DllImport("gdiplus.dll", CharSet = CharSet.Unicode)]
        private static extern int GdipLoadImageFromFile(string filename, out IntPtr image);

        // Loads an Image from a File by invoking GDI Plus instead of using build-in 
        // .NET methods, or falls back to Image.FromFile. GDI Plus should be faster.
        public static Image LoadImageFastFromFile(string filename) {
            IntPtr imagePtr = IntPtr.Zero;
            Image image = null;

            try {
                if (GdipLoadImageFromFile(filename, out imagePtr) != 0) {
                    //logger.Warn("gdiplus.dll method failed. Will degrade performance.");
                    image = Image.FromFile(filename);
                }

                else 
                    image = (Image)typeof(Bitmap).InvokeMember("FromGDIplus", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod, null, null, new object[] { imagePtr });
            }
            catch (Exception) {
                //logger.Error("Failed to load image from " + filename);
                image = null;
            }

            return image;

        }

    }
    
}*/
