/*
 * This class is converted VB.Net from the CScItem
 * from http://www.codeproject.com/KB/cpp/VbNetExpTree.aspx 
 */

using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System;
using System.Collections;
using System.Windows.Forms;
using System.Diagnostics;
using System.Management;

public class ShellItem : IDisposable, IComparable
{
    
    #region "   Shared Private Fields"
    //This class has occasion to refer to the TypeName as reported by
    // ShellDll.SHGetFileInfo. It needs to compare this to the string
    // (in English) "System Folder"
    //on non-English systems, we do not know, in the general case,
    // what the equivalent string is to compare against
    //The following variable is set by Sub New() to the string that
    // corresponds to "System Folder" on the current machine
    // Sub New() depends on the existance of My Computer(ShellDll.CSIDL.DRIVES),
    // to determine what the equivalent string is
    private static string m_strSystemFolder;
    
    //My Computer is also commonly used (though not internally),
    // so save & expose its name on the current machine
    private static string m_strMyComputer;
    
    //To get My Documents sorted first, we need to know the Locale 
    //specific name of that folder.
    private static string m_strMyDocuments;
    
    // The DesktopBase is set up via Sub New() (one time only) and
    //  disposed of only when DesktopBase is finally disposed of
    private static ShellItem DesktopBase;
    
    //We can avoid an extra ShellDll.SHGetFileInfo call once this is set up
    private static int OpenFolderIconIndex = -1;
    
    // It is also useful to know if the OS is XP or above.  
    // Set up in Sub New() to avoid multiple calls to find this info
    private static bool XPorAbove;
    // Likewise if OS is Win2K or Above
    private static bool Win2KOrAbove;
    
    // DragDrop, possibly among others, needs to know the Path of
    // the DeskTopDirectory in addition to the Desktop itself
    // Also need the actual ShellItem for the DeskTopDirectory, so get it
    private static ShellItem m_DeskTopDirectory;
    
    
    #endregion
    
    #region "   Instance Private Fields"
    //m_Folder and m_Pidl must be released/freed at Dispose time
    private ShellDll.IShellFolder m_Folder;
    //if item is a folder, contains the Folder interface for this instance
    private IntPtr m_Pidl;
    //The Absolute PIDL for this item (not retained for files)
    private string m_DisplayName = "";
    private string m_Path;
    private string m_TypeName;
    private ShellItem m_Parent;
    //= Nothing
    private int m_IconIndexNormal;
    //index into the System Image list for Normal icon
    private int m_IconIndexOpen;
    //index into the SystemImage list for Open icon
    private bool m_IsBrowsable;
    private bool m_IsFileSystem;
    private bool m_IsFolder;
    private bool m_HasSubFolders;
    private bool m_IsLink;
    private bool m_IsDisk;
    private bool m_IsShared;
    private bool m_IsHidden;
    private bool m_IsNetWorkDrive;
    //= False
    private bool m_IsRemovable;
    //= False
    private bool m_IsReadOnly;
    //= False
    //Properties of interest to Drag Operations
    private bool m_CanMove;
    //= False
    private bool m_CanCopy;
    //= False
    private bool m_CanDelete;
    //= False
    private bool m_CanLink;
    //= False
    private bool m_IsDropTarget;
    //= False
    private ShellDll.SFGAO m_Attributes;
    //the original, returned from GetAttributesOf
    
    private int m_SortFlag;
    //= 0 'Used in comparisons
    
    private ArrayList m_Directories;
    
    //The following elements are only filled in on demand
    private bool m_XtrInfo;
    //= False
    private DateTime m_LastWriteTime;
    private DateTime m_CreationTime;
    private DateTime m_LastAccessTime;
    private long m_Length;
    
    //Indicates whether DisplayName, TypeName, SortFlag have been set up
    private bool m_HasDispType;
    //= False
    
    //Indicates whether IsReadOnly has been set up
    private bool m_IsReadOnlySetup;
    //= False
    
    //Holds a byte() representation of m_PIDL -- filled when needed
    private cPidl m_cPidl;
    
    //Flags for Dispose state
    private bool m_Disposed;
    
    #endregion
    
    #region "   Destructor"
    /// <summary>
    /// Summary of Dispose.
    /// </summary>
    /// 
    public void Dispose()
    {
        Dispose(true);
        // Take yourself off of the finalization queue
        // to prevent finalization code for this object
        // from executing a second time.
        GC.SuppressFinalize(this);
    }
    /// <summary>
    /// Deallocates CoTaskMem contianing m_Pidl and removes reference to m_Folder
    /// </summary>
    /// <param name="disposing"></param>
    /// 
    protected virtual void Dispose(bool disposing)
    {
        // Allow your Dispose method to be called multiple times,
        // but throw an exception if the object has been disposed.
        // Whenever you do something with this class, 
        // check to see if it has been disposed.
        if (!(m_Disposed)) {
            // If disposing equals true, dispose all managed 
            // and unmanaged resources.
            m_Disposed = true;
            if ((disposing)) {
            }
            // Release unmanaged resources. If disposing is false,
            // only the following code is executed. 
            if ((m_Folder != null)) {
                Marshal.ReleaseComObject(m_Folder);
            }
            if (!m_Pidl.Equals(IntPtr.Zero)) {
                Marshal.FreeCoTaskMem(m_Pidl);
            }
        }
        else {
            throw new Exception("ShellItem Disposed more than once");
        }
    }
    
    // This Finalize method will run only if the 
    // Dispose method does not get called.
    // By default, methods are NotOverridable. 
    // This prevents a derived class from overriding this method.
    /// <summary>
    /// Summary of Finalize.
    /// </summary>
    /// 
    ~ShellItem()
    {
        // Do not re-create Dispose clean-up code here.
        // Calling Dispose(false) is optimal in terms of
        // readability and maintainability.
        Dispose(false);
    }
    
    #endregion
    
    #region "   Constructors"
    
    #region "       Private Sub New(ByVal folder As ShellDll.IShellFolder, ByVal pidl As IntPtr, ByVal parent As ShellItem)"
    /// <summary>
    /// Private Constructor, creates new ShellItem from the item's parent folder and
    ///  the item's PIDL relative to that folder.</summary>
    /// <param name="folder">the folder interface of the parent</param>
    /// <param name="pidl">the Relative PIDL of this item</param>
    /// <param name="parent">the CShitem of the parent</param>
    /// 
    private ShellItem(ShellDll.IShellFolder folder, IntPtr pidl, ShellItem parent)
    {
        if ((DesktopBase == null)) {
                //This initializes the Desktop folder
            DesktopBase = new ShellItem();
        }
        m_Parent = parent;
        m_Pidl = concatPidls(parent.PIDL, pidl);
        
        //Get some attributes
        SetUpAttributes(folder, pidl);
        
        //Set unfetched value for IconIndex....
        m_IconIndexNormal = -1;
        m_IconIndexOpen = -1;
        //finally, set up my Folder
        if (m_IsFolder) {
            int HR = 0;
            HR = folder.BindToObject(pidl, IntPtr.Zero, ref ShellDll.IID_IShellFolder, ref m_Folder);
            if (HR != ShellDll.NOERROR) {
                Marshal.ThrowExceptionForHR(HR);
            }
        }
    }
    #endregion
    
    #region "       Sub New()"
    /// <summary>
    /// Private Constructor. Creates ShellItem of the Desktop
    /// </summary>
    /// 
    private ShellItem()
    {
        //only used when desktopfolder has not been intialized
        if ((DesktopBase != null)) {
            throw new Exception("Attempt to initialize ShellItem for second time");
        }
        
        int HR = 0;
        //firstly determine what the local machine calls a "System Folder" and "My Computer"
        IntPtr tmpPidl = default(IntPtr);
        HR = ShellDll.SHGetSpecialFolderLocation(0, (int)ShellDll.CSIDL.DRIVES, ref tmpPidl);
        ShellDll.SHFILEINFO shfi = new ShellDll.SHFILEINFO();
        int dwflag = (int)(ShellDll.SHGFI.DISPLAYNAME | ShellDll.SHGFI.TYPENAME | ShellDll.SHGFI.PIDL);
        int dwAttr = 0;
		ShellDll.SHGetFileInfo(tmpPidl, dwAttr, ref shfi, (int)ShellDll.cbFileInfo, dwflag);
        m_strSystemFolder = shfi.szTypeName;
        m_strMyComputer = shfi.szDisplayName;
        Marshal.FreeCoTaskMem(tmpPidl);
        //set OS version info
        XPorAbove = ShellDll.IsXpOrAbove();
        Win2KOrAbove = ShellDll.Is2KOrAbove();
        
        //With That done, now set up Desktop ShellItem
		m_Path = "::{" + ShellDll.DesktopGUID.ToString() + "}";
        m_IsFolder = true;
        m_HasSubFolders = true;
        m_IsBrowsable = false;
		HR = ShellDll.SHGetDesktopFolder(ref m_Folder);
		m_Pidl = ShellDll.GetSpecialFolderLocation(IntPtr.Zero, (int)ShellDll.CSIDL.DESKTOP);
        dwflag = (int)(ShellDll.SHGFI.DISPLAYNAME | ShellDll.SHGFI.TYPENAME | ShellDll.SHGFI.SYSICONINDEX | ShellDll.SHGFI.PIDL);
        dwAttr = 0;
		IntPtr H = ShellDll.SHGetFileInfo(m_Pidl, dwAttr, ref shfi, (int)ShellDll.cbFileInfo, dwflag);
        m_DisplayName = shfi.szDisplayName;
        m_TypeName = strSystemFolder;
        //not returned correctly by ShellDll.SHGetFileInfo
        m_IconIndexNormal = shfi.iIcon;
        m_IconIndexOpen = shfi.iIcon;
        m_HasDispType = true;
        m_IsDropTarget = true;
        m_IsReadOnly = false;
        m_IsReadOnlySetup = true;
        
        //also get local name for "My Documents"
        int pchEaten = 0;
        tmpPidl = IntPtr.Zero;
		int tmp = 0;
        HR = m_Folder.ParseDisplayName(0, IntPtr.Zero, "::{450d8fba-ad25-11d0-98a8-0800361b1103}", ref pchEaten, ref tmpPidl, ref tmp);
        shfi = new ShellDll.SHFILEINFO();
        dwflag = (int)(ShellDll.SHGFI.DISPLAYNAME | ShellDll.SHGFI.TYPENAME | ShellDll.SHGFI.PIDL);
        dwAttr = 0;
		ShellDll.SHGetFileInfo(tmpPidl, dwAttr, ref shfi, ShellDll.cbFileInfo, dwflag);
        m_strMyDocuments = shfi.szDisplayName;
        Marshal.FreeCoTaskMem(tmpPidl);
        //this must be done after getting "My Documents" string
        m_SortFlag = ComputeSortFlag();
        //Set DesktopBase
        DesktopBase = this;
        // Lastly, get the Path and ShellItem of the DesktopDirectory -- useful for DragDrop
        m_DeskTopDirectory = new ShellItem(ShellDll.CSIDL.DESKTOPDIRECTORY);
    }
    #endregion
    
    #region "       New(ByVal ID As ShellDll.CSIDL)"
    /// <summary>Create instance based on a non-desktop ShellDll.CSIDL.
    /// Will create based on any ShellDll.CSIDL Except the DeskTop ShellDll.CSIDL</summary>
    /// <param name="ID">Value from ShellDll.CSIDL enumeration denoting the folder to create this ShellItem of.</param>
    /// 
    public ShellItem(ShellDll.CSIDL ID)
    {
        if ((DesktopBase == null)) {
                //This initializes the Desktop folder
            DesktopBase = new ShellItem();
        }
        int HR = 0;
        if (ID == ShellDll.CSIDL.MYDOCUMENTS) {
            int pchEaten = 0;
			int tmp = 0;
            HR = DesktopBase.m_Folder.ParseDisplayName(0, IntPtr.Zero, "::{450d8fba-ad25-11d0-98a8-0800361b1103}", ref pchEaten, ref m_Pidl, ref tmp);
        }
        else {
            HR = ShellDll.SHGetSpecialFolderLocation(0, (int)ID, ref m_Pidl);
        }
        if (HR == ShellDll.NOERROR) {
            ShellDll.IShellFolder pParent = default(ShellDll.IShellFolder);
            IntPtr relPidl = IntPtr.Zero;

			pParent = GetParentOf(m_Pidl, ref relPidl);
            //Get the Attributes
            SetUpAttributes(pParent, relPidl);
            //Set unfetched value for IconIndex....
            m_IconIndexNormal = -1;
            m_IconIndexOpen = -1;
            //finally, set up my Folder
            if (m_IsFolder) {
                HR = pParent.BindToObject(relPidl, IntPtr.Zero, ref ShellDll.IID_IShellFolder, ref m_Folder);
                if (HR != ShellDll.NOERROR) {
                    Marshal.ThrowExceptionForHR(HR);
                }
            }
            Marshal.ReleaseComObject(pParent);
            //if PidlCount(m_Pidl) = 1 then relPidl is same as m_Pidl, don't release
            if (PidlCount(m_Pidl) > 1) Marshal.FreeCoTaskMem(relPidl); 
        }
        else {
            Marshal.ThrowExceptionForHR(HR);
        }
    }
    #endregion
    
    #region "       New(ByVal path As String)"
    /// <summary>Create a new ShellItem based on a Path Must be a valid FileSystem Path</summary>
    /// <param name="path"></param>
    /// 
    public ShellItem(string path)
    {
        if ((DesktopBase == null)) {
                //This initializes the Desktop folder
            DesktopBase = new ShellItem();
        }
        //Removal of following code allows Path(GUID) of Special FOlders to serve
        //  as a valid Path for ShellItem creation (part of Calum's refresh code needs this
        //If Not Directory.Exists(path) AndAlso Not File.Exists(path) Then
        //    Throw New Exception("ShellItem -- Invalid Path specified")
        //End If
        int HR = 0;
		int tmp1 = 0, tmp2 = 0;
        HR = DesktopBase.m_Folder.ParseDisplayName(0, IntPtr.Zero, path, ref tmp1, ref m_Pidl, ref tmp2);
        if (!(HR == ShellDll.NOERROR)) Marshal.ThrowExceptionForHR(HR); 
        ShellDll.IShellFolder pParent = default(ShellDll.IShellFolder);
        IntPtr relPidl = IntPtr.Zero;
        
        pParent = GetParentOf(m_Pidl, ref relPidl);
        
        //Get the Attributes
        SetUpAttributes(pParent, relPidl);
        //Set unfetched value for IconIndex....
        m_IconIndexNormal = -1;
        m_IconIndexOpen = -1;
        //finally, set up my Folder
        if (m_IsFolder) {
            HR = pParent.BindToObject(relPidl, IntPtr.Zero, ref ShellDll.IID_IShellFolder, ref m_Folder);
            if (HR != ShellDll.NOERROR) {
                Marshal.ThrowExceptionForHR(HR);
            }
        }
        Marshal.ReleaseComObject(pParent);
        //if PidlCount(m_Pidl) = 1 then relPidl is same as m_Pidl, don't release
        if (PidlCount(m_Pidl) > 1) {
            Marshal.FreeCoTaskMem(relPidl);
        }
    }
    #endregion

	#region "       ShellItem(byte[] FoldBytes, byte[] ItemBytes)"
	///<Summary>Given a Byte() containing the Pidl of the parent
	/// folder and another Byte() containing the Pidl of the Item,
	/// relative to the Folder, Create a ShellItem for the Item.
	/// This is of primary use in dealing with "Shell IDList Array" 
	/// formatted info passed in a Drag Operation
	/// </Summary>
	public ShellItem(byte[] FoldBytes, byte[] ItemBytes)
	{
        if ((DesktopBase == null)) {
                //This initializes the Desktop folder
            DesktopBase = new ShellItem();
        }
        ShellDll.IShellFolder pParent = MakeFolderFromBytes(FoldBytes);
        IntPtr ipParent = cPidl.BytesToPidl(FoldBytes);
        IntPtr ipItem = cPidl.BytesToPidl(ItemBytes);
        if ((pParent == null)) {
                //m_PIDL will = IntPtr.Zero for really bad CShitem
            goto XIT;
        }
        if (ipParent.Equals(IntPtr.Zero) | ipItem.Equals(IntPtr.Zero)) {
            goto XIT;
        }
        // Now process just like sub new(folder,pidl,parent) version
        m_Pidl = concatPidls(ipParent, ipItem);
        
        //Get some attributes
        SetUpAttributes(pParent, ipItem);
        
        //Set unfetched value for IconIndex....
        m_IconIndexNormal = -1;
        m_IconIndexOpen = -1;
        //finally, set up my Folder
        if (m_IsFolder) {
            int HR = 0;
            HR = pParent.BindToObject(ipItem, IntPtr.Zero, ref ShellDll.IID_IShellFolder, ref m_Folder);
            #if Debug
            if (HR != NOERROR) {
                Marshal.ThrowExceptionForHR(HR);
            }
            #endif
        }
        XIT:
        //On any kind of exit, free the allocated memory
        #if Debug
        if (m_Pidl.Equals(IntPtr.Zero)) {
            Debug.WriteLine("CShItem.New(FoldBytes,ItemBytes) Failed");
        }
        else {
            Debug.WriteLine("CShItem.New(FoldBytes,ItemBytes) Created " + this.Path);
        }
        #endif
        if (!ipParent.Equals(IntPtr.Zero)) {
            Marshal.FreeCoTaskMem(ipParent);
        }
        if (!ipItem.Equals(IntPtr.Zero)) {
            Marshal.FreeCoTaskMem(ipItem);
        }
	}

	#endregion
    
    #region "       Utility functions used in Constructors"
    
    #region "       IsValidPidl"
    ///<Summary>It is impossible to validate a PIDL completely since its contents
    /// are arbitrarily defined by the creating Shell Namespace.  However, it
    /// is possible to validate the structure of a PIDL.</Summary>
    public static bool IsValidPidl(byte[] b)
    {
        bool IsValidPidl = false;
        //assume failure
        int bMax = b.Length - 1;
        //max value that index can have
		if (bMax < 1) return IsValidPidl;
 
        //min Size is 2 bytes
        int cb = b[0] + (b[1] * 256);
        int indx = 0;
        while (cb > 0) {
			if ((indx + cb + 1) > bMax) return IsValidPidl;
 
            //an error
            indx += cb;
            cb = b[indx] + (b[indx + 1] * 256);
        }
            // on fall thru, it is ok as far as we can check
        return true;
    }
    #endregion
    
    #region "   MakeFolderFromBytes"
    public static ShellDll.IShellFolder MakeFolderFromBytes(byte[] b)
    {
        ShellDll.IShellFolder functionReturnValue = default(ShellDll.IShellFolder);
        functionReturnValue = null;
        //get rid of VS2005 warning
        if (!IsValidPidl(b)) return null; 
        if (b.Length == 2 && ((b[0] == 0) & (b[1] == 0))) {
            //this is the desktop
            return DesktopBase.Folder;
        }
        else if (b.Length == 0) {
            //Also indicates the desktop
            return DesktopBase.Folder;
        }
        else {
            IntPtr ptr = Marshal.AllocCoTaskMem(b.Length);
            if (ptr.Equals(IntPtr.Zero)) return null; 
            Marshal.Copy(b, 0, ptr, b.Length);
            //the next statement assigns a IshellFolder object to the function return, or has an error
			int hr = DesktopBase.Folder.BindToObject(ptr, IntPtr.Zero, ref ShellDll.IID_IShellFolder, ref functionReturnValue);
            if (hr != 0) functionReturnValue = null; 
            Marshal.FreeCoTaskMem(ptr);
        }
        return functionReturnValue;
    }
    #endregion
    
    #region "           GetParentOf"
    
    ///<Summary>Returns both the ShellDll.IShellFolder interface of the parent folder
    ///  and the relative pidl of the input PIDL</Summary>
    ///<remarks>Several internal functions need this information and do not have
    /// it readily available. GetParentOf serves those functions</remarks>
    private static ShellDll.IShellFolder GetParentOf(IntPtr pidl, ref IntPtr relPidl)
    {
        ShellDll.IShellFolder functionReturnValue = default(ShellDll.IShellFolder);
        functionReturnValue = null;
        //avoid VB2005 warning
        int HR = 0;
        int itemCnt = PidlCount(pidl);
        if (itemCnt == 1) {
            //parent is desktop
			HR = ShellDll.SHGetDesktopFolder(ref functionReturnValue);
            relPidl = pidl;
        }
        else {
            IntPtr tmpPidl = default(IntPtr);
            tmpPidl = TrimPidl(pidl, ref relPidl);
            HR = DesktopBase.m_Folder.BindToObject(tmpPidl, IntPtr.Zero, ref ShellDll.IID_IShellFolder, ref functionReturnValue);
            Marshal.FreeCoTaskMem(tmpPidl);
        }
        if (!(HR == ShellDll.NOERROR)) Marshal.ThrowExceptionForHR(HR); 
        return functionReturnValue;
    }
    #endregion
    
    #region "           SetUpAttributes"
    /// <summary>Get the base attributes of the folder/file that this ShellItem represents</summary>
    /// <param name="folder">Parent Folder of this Item</param>
    /// <param name="pidl">Relative Pidl of this Item.</param>
    /// 
    private void SetUpAttributes(ShellDll.IShellFolder folder, IntPtr pidl)
    {
        ShellDll.SFGAO attrFlag = default(ShellDll.SFGAO);
        attrFlag = ShellDll.SFGAO.BROWSABLE;
        attrFlag = attrFlag | ShellDll.SFGAO.FILESYSTEM;
        attrFlag = attrFlag | ShellDll.SFGAO.HASSUBFOLDER;
        attrFlag = attrFlag | ShellDll.SFGAO.FOLDER;
        attrFlag = attrFlag | ShellDll.SFGAO.LINK;
        attrFlag = attrFlag | ShellDll.SFGAO.SHARE;
        attrFlag = attrFlag | ShellDll.SFGAO.HIDDEN;
        attrFlag = attrFlag | ShellDll.SFGAO.REMOVABLE;
        //attrFlag = attrFlag Or ShellDll.SFGAO.RDONLY   'made into an on-demand attribute
        attrFlag = attrFlag | ShellDll.SFGAO.CANCOPY;
        attrFlag = attrFlag | ShellDll.SFGAO.CANDELETE;
        attrFlag = attrFlag | ShellDll.SFGAO.CANLINK;
        attrFlag = attrFlag | ShellDll.SFGAO.CANMOVE;
        attrFlag = attrFlag | ShellDll.SFGAO.DROPTARGET;
        //Note: for GetAttributesOf, we must provide an array, in  all cases with 1 element
        IntPtr[] aPidl = new IntPtr[1];
        aPidl[0] = pidl;
        folder.GetAttributesOf(1, aPidl, ref attrFlag);
        m_Attributes = attrFlag;
        m_IsBrowsable = ((attrFlag & ShellDll.SFGAO.BROWSABLE) != 0);
        m_IsFileSystem = ((attrFlag & ShellDll.SFGAO.FILESYSTEM) != 0);
        m_HasSubFolders = ((attrFlag & ShellDll.SFGAO.HASSUBFOLDER) != 0);
        m_IsFolder = ((attrFlag & ShellDll.SFGAO.FOLDER) != 0);
        m_IsLink = ((attrFlag & ShellDll.SFGAO.LINK) != 0);
        m_IsShared = ((attrFlag & ShellDll.SFGAO.SHARE) != 0);
        m_IsHidden = ((attrFlag & ShellDll.SFGAO.HIDDEN) != 0);
        m_IsRemovable = ((attrFlag & ShellDll.SFGAO.REMOVABLE) != 0);
        //m_IsReadOnly = CBool(attrFlag And ShellDll.SFGAO.RDONLY)      'made into an on-demand attribute
        m_CanCopy = ((attrFlag & ShellDll.SFGAO.CANCOPY) != 0);
        m_CanDelete = ((attrFlag & ShellDll.SFGAO.CANDELETE) != 0);
        m_CanLink = ((attrFlag & ShellDll.SFGAO.CANLINK) != 0);
        m_CanMove = ((attrFlag & ShellDll.SFGAO.CANMOVE) != 0);
		m_IsDropTarget = ((attrFlag & ShellDll.SFGAO.DROPTARGET) != 0);
        
        //Get the Path
        IntPtr strr = Marshal.AllocCoTaskMem(ShellDll.MAX_PATH * 2 + 4);
        Marshal.WriteInt32(strr, 0, 0);
        StringBuilder buf = new StringBuilder(ShellDll.MAX_PATH);
        ShellDll.SHGDN itemflags = ShellDll.SHGDN.FORPARSING;
        folder.GetDisplayNameOf(pidl, itemflags, strr);
        int HR = ShellDll.StrRetToBuf(strr, pidl, buf, ShellDll.MAX_PATH);
        Marshal.FreeCoTaskMem(strr);
        //now done with it
        if (HR == ShellDll.NOERROR) {
            m_Path = buf.ToString();
            //check for zip file = folder on xp, leave it a file
            if (m_IsFolder && m_IsFileSystem && XPorAbove) {
                //Note:meaning of ShellDll.SFGAO.STREAM changed between win2k and winXP
                //Version 20 code
                //If File.Exists(m_Path) Then
                //    m_IsFolder = False
                //End If
                //Version 21 code
                aPidl[0] = pidl;
                attrFlag = ShellDll.SFGAO.STREAM;
                folder.GetAttributesOf(1, aPidl, ref attrFlag);
                if ((attrFlag & ShellDll.SFGAO.STREAM) != 0) {
                    m_IsFolder = false;
                }
            }
            if (m_Path.Length == 3 && m_Path.Substring(1).Equals(":\\")) {
                m_IsDisk = true;
            }
        }
        else {
            Marshal.ThrowExceptionForHR(HR);
        }
    }
    
    #endregion
    
    #endregion
    
    #region "       Public Shared Function GetCShItem(ByVal path As String) As ShellItem"
    public static ShellItem GetCShItem(string path)
    {
        ShellItem functionReturnValue = default(ShellItem);
        functionReturnValue = null;
        //assume failure
        int HR = 0;
        IntPtr tmpPidl = default(IntPtr);
		int tmp1 = 0, tmp2 = 0;
        HR = GetDeskTop().Folder.ParseDisplayName(0, IntPtr.Zero, path, ref tmp1, ref tmpPidl, ref tmp2);
        if (HR == 0) {
            functionReturnValue = FindCShItem(tmpPidl);
			if ((functionReturnValue == null))
			{
                try {
                    functionReturnValue = new ShellItem(path);
                }
                catch {
                    functionReturnValue = null;
                }
            }
        }
        if (!tmpPidl.Equals(IntPtr.Zero)) {
            Marshal.FreeCoTaskMem(tmpPidl);
        }
        return functionReturnValue;
    }
    #endregion
    
    #region "       Public Shared Function GetCShItem(ByVal ID As ShellDll.CSIDL) As ShellItem"
    public static ShellItem GetCShItem(ShellDll.CSIDL ID)
    {
        ShellItem functionReturnValue = default(ShellItem);
        functionReturnValue = null;
        //avoid VB2005 Warning
        if (ID == ShellDll.CSIDL.DESKTOP) {
            return GetDeskTop();
        }
        int HR = 0;
        IntPtr tmpPidl = default(IntPtr);
        if (ID == ShellDll.CSIDL.MYDOCUMENTS) {
            int pchEaten = 0;
			int tmp = 0;
            HR = GetDeskTop().Folder.ParseDisplayName(0, IntPtr.Zero, "::{450d8fba-ad25-11d0-98a8-0800361b1103}", ref pchEaten, ref tmpPidl, ref tmp);
        }
        else {
            HR = ShellDll.SHGetSpecialFolderLocation(0, (int)ID, ref tmpPidl);
        }
        if (HR == ShellDll.NOERROR) {
            functionReturnValue = FindCShItem(tmpPidl);
			if ((functionReturnValue == null))
			{
                try {
                    functionReturnValue = new ShellItem(ID);
                }
                catch {
                    functionReturnValue = null;
                }
            }
        }
        if (!tmpPidl.Equals(IntPtr.Zero)) {
            Marshal.FreeCoTaskMem(tmpPidl);
        }
        return functionReturnValue;
    }
    #endregion
    
    #region "       Public Shared Function GetCShItem(ByVal FoldBytes() As Byte, ByVal ItemBytes() As Byte) As ShellItem"
    public static ShellItem GetCShItem(byte[] FoldBytes, byte[] ItemBytes)
    {
        ShellItem functionReturnValue = default(ShellItem);
        functionReturnValue = null;
        //assume failure
        byte[] b = cPidl.JoinPidlBytes(FoldBytes, ItemBytes);
        if ((b == null)) return functionReturnValue;
 
        //can do no more with invalid pidls
        //otherwise do like below, skipping unnecessary validation check
        IntPtr thisPidl = Marshal.AllocCoTaskMem(b.Length);
        if (thisPidl.Equals(IntPtr.Zero)) return null; 
        Marshal.Copy(b, 0, thisPidl, b.Length);
        functionReturnValue = FindCShItem(thisPidl);
        Marshal.FreeCoTaskMem(thisPidl);
		if ((functionReturnValue == null))
		{
            //didn't find it, make new
            try {
                functionReturnValue = new ShellItem(FoldBytes, ItemBytes);
            }
            catch {
                
            }
        }
        if (functionReturnValue.PIDL.Equals(IntPtr.Zero)) functionReturnValue = null; 
        return functionReturnValue;
    }
    #endregion
    
    #region "       Public Shared Function FindCShItem(ByVal b() As Byte) As ShellItem"
    public static ShellItem FindCShItem(byte[] b)
    {
        ShellItem functionReturnValue = default(ShellItem);
        if (!IsValidPidl(b)) return null; 
        IntPtr thisPidl = Marshal.AllocCoTaskMem(b.Length);
        if (thisPidl.Equals(IntPtr.Zero)) return null; 
        Marshal.Copy(b, 0, thisPidl, b.Length);
        functionReturnValue = FindCShItem(thisPidl);
        Marshal.FreeCoTaskMem(thisPidl);
        return functionReturnValue;
    }
    #endregion
    
    #region "       Public Shared Function FindCShItem(ByVal ptr As IntPtr) As ShellItem"
    public static ShellItem FindCShItem(IntPtr ptr)
    {
        ShellItem functionReturnValue = default(ShellItem);
        functionReturnValue = null;
        //avoid VB2005 Warning
        ShellItem BaseItem = ShellItem.GetDeskTop();
        //ShellItem CSI = default(ShellItem);
        bool FoundIt = false;
        //True if we found item or an ancestor
        while (!(FoundIt)) {
            foreach (var obj in BaseItem.GetDirectories(true)) {
				ShellItem CSI = obj as ShellItem;
                if (IsAncestorOf(CSI.PIDL, ptr, false)) {
                    if (ShellItem.IsEqual(CSI.PIDL, ptr)) {
                        //we found the desired item
                        return CSI;
                    }
                    else {
                        BaseItem = CSI;
                        FoundIt = true;
                        break; // TODO: might not be correct. Was : Exit For
                    }
                }
            }
            if (!FoundIt) return null; 
            //didn't find an ancestor
            //The complication is that the desired item may not be a directory
            if (!IsAncestorOf(BaseItem.PIDL, ptr, true)) {
                //Don't have immediate ancestor
                    //go around again
                FoundIt = false;
            }
            else {
                foreach (var obj in BaseItem.GetItems()) {
					ShellItem CSI = obj as ShellItem;
					if (ShellItem.IsEqual(CSI.PIDL, ptr))
					{
                        return CSI;
                    }
                }
                //fall thru here means it doesn't exist or we can't find it because of funny PIDL from SHParseDisplayName
                return null;
            }
        }
        return functionReturnValue;
    }
    #endregion
    
    #endregion
    
    #region "   Icomparable -- for default Sorting"
    
    /// <summary>Computes the Sort key of this ShellItem, based on its attributes</summary>
    /// 
    private int ComputeSortFlag()
    {
        int rVal = 0;
        if (m_IsDisk) rVal = 0x100000; 
        if (m_TypeName.Equals(strSystemFolder)) {
            if (!m_IsBrowsable) {
                rVal = rVal | 0x10000;
                if (m_strMyDocuments.Equals(m_DisplayName)) {
                    rVal = rVal | 0x1;
                }
            }
            else {
                rVal = rVal | 0x1000;
            }
        }
        if (m_IsFolder) rVal = rVal | 0x100; 
        return rVal;
    }
    
    ///<Summary> CompareTo(obj as object)
    ///  Compares obj to this instance based on SortFlag-- obj must be a ShellItem</Summary>
    ///<SortOrder>  (low)Disks,non-browsable System Folders,
    ///              browsable System Folders, 
    ///               Directories, Files, Nothing (high)</SortOrder>
    public virtual int CompareTo(object obj)
    {
        ShellItem Other = obj as ShellItem;
        if ((Other == null)) return 1; 
        //non-existant is always low
        if (!m_HasDispType) SetDispType(); 
        int cmp = Other.SortFlag - m_SortFlag;
        //Note the reversal
        if (cmp != 0) {
            return cmp;
        }
        else {
            if (m_IsDisk) {
                //implies that both are
                return string.Compare(m_Path, Other.Path);
            }
            else {
                return string.Compare(m_DisplayName, Other.DisplayName);
            }
        }
    }
    #endregion
    
    #region "   Properties"
    
    #region "       Shared Properties"
    public static string strMyComputer {
        get { return m_strMyComputer; }
    }
    
    public static string strSystemFolder {
        get { return m_strSystemFolder; }
    }
    
    public static string DesktopDirectoryPath {
        get { return m_DeskTopDirectory.Path; }
    }
    
    #endregion
    
    #region "       Normal Properties"
    public IntPtr PIDL {
        get { return m_Pidl; }
    }
    
    public ShellDll.IShellFolder Folder {
        get { return m_Folder; }
    }
    
    public string Path {
        get { return m_Path; }
    }
    public ShellItem Parent {
        get { return m_Parent; }
    }
    
    public ShellDll.SFGAO Attributes {
        get { return m_Attributes; }
    }
    public bool IsBrowsable {
        get { return m_IsBrowsable; }
    }
    public bool IsFileSystem {
        get { return m_IsFileSystem; }
    }
    public bool IsFolder {
        get { return m_IsFolder; }
    }
    public bool HasSubFolders {
        get { return m_HasSubFolders; }
    }
    public bool IsDisk {
        get { return m_IsDisk; }
    }
    public bool IsLink {
        get { return m_IsLink; }
    }
    public bool IsShared {
        get { return m_IsShared; }
    }
    public bool IsHidden {
        get { return m_IsHidden; }
    }
    public bool IsRemovable {
        get { return m_IsRemovable; }
    }
    
    #region "       Drag Ops Properties"
    public bool CanMove {
        get { return m_CanMove; }
    }
    public bool CanCopy {
        get { return m_CanCopy; }
    }
    public bool CanDelete {
        get { return m_CanDelete; }
    }
    public bool CanLink {
        get { return m_CanLink; }
    }
    public bool IsDropTarget {
        get { return m_IsDropTarget; }
    }
    #endregion
    
    #endregion
    
    #region "       Filled on Demand Properties"
    
    #region "           Filled based on m_HasDispType"
    /// <summary>
    /// Set DisplayName, TypeName, and SortFlag when actually needed
    /// </summary>
    /// 
    private void SetDispType()
    {
        //Get Displayname, TypeName
        ShellDll.SHFILEINFO shfi = new ShellDll.SHFILEINFO();
        int dwflag = (int)(ShellDll.SHGFI.DISPLAYNAME | ShellDll.SHGFI.TYPENAME | ShellDll.SHGFI.PIDL);
        int dwAttr = 0;
        if (m_IsFileSystem & !m_IsFolder) {
            dwflag = dwflag | (int)ShellDll.SHGFI.USEFILEATTRIBUTES;
			dwAttr = (int)ShellDll.FILE_ATTRIBUTE_NORMAL;
        }
		IntPtr H = ShellDll.SHGetFileInfo(m_Pidl, dwAttr, ref shfi, ShellDll.cbFileInfo, dwflag);
        m_DisplayName = shfi.szDisplayName;
        m_TypeName = shfi.szTypeName;
        //fix DisplayName
        if (m_DisplayName.Equals("")) {
            m_DisplayName = m_Path;
        }
        //Fix TypeName
        //If m_IsFolder And m_TypeName.Equals("File") Then
        //    m_TypeName = "File Folder"
        //End If
        m_SortFlag = ComputeSortFlag();
        m_HasDispType = true;
    }
    
    public string DisplayName {
        get {
            if (!m_HasDispType) SetDispType(); 
            return m_DisplayName;
        }
    }
    
    private int SortFlag {
        get {
            if (!m_HasDispType) SetDispType(); 
            return m_SortFlag;
        }
    }
    
    public string TypeName {
        get {
            if (!m_HasDispType) SetDispType(); 
            return m_TypeName;
        }
    }
    #endregion
    
    #region "           IconIndex properties"
    public int IconIndexNormal {
        get {
            if (m_IconIndexNormal < 0) {
                if (!m_HasDispType) SetDispType(); 
                ShellDll.SHFILEINFO shfi = new ShellDll.SHFILEINFO();
                int dwflag = (int)(ShellDll.SHGFI.PIDL | ShellDll.SHGFI.SYSICONINDEX);
                int dwAttr = 0;
                if (m_IsFileSystem & !m_IsFolder) {
                    dwflag = dwflag | (int)ShellDll.SHGFI.USEFILEATTRIBUTES;
                    dwAttr = (int)ShellDll.FILE_ATTRIBUTE_NORMAL;
                }
                IntPtr H = ShellDll.SHGetFileInfo(m_Pidl, dwAttr, ref shfi, ShellDll.cbFileInfo, dwflag);
                m_IconIndexNormal = shfi.iIcon;
            }
            return m_IconIndexNormal;
        }
    }
    // IconIndexOpen is Filled on demand
    public int IconIndexOpen {
        get {
            if (m_IconIndexOpen < 0) {
                if (!m_HasDispType) SetDispType(); 
                if (!m_IsDisk & m_IsFileSystem & m_IsFolder) {
                    if (OpenFolderIconIndex < 0) {
                        int dwflag = (int)(ShellDll.SHGFI.SYSICONINDEX | ShellDll.SHGFI.PIDL);
                        ShellDll.SHFILEINFO shfi = new ShellDll.SHFILEINFO();
                        IntPtr H = ShellDll.SHGetFileInfo(m_Pidl, 0, ref shfi, ShellDll.cbFileInfo, dwflag | (int)ShellDll.SHGFI.OPENICON);
                        m_IconIndexOpen = shfi.iIcon;
                    }
                    else {
                        //If m_TypeName.Equals("File Folder") Then
                        //    OpenFolderIconIndex = shfi.iIcon
                        //End If
                        m_IconIndexOpen = OpenFolderIconIndex;
                    }
                }
                else {
                    m_IconIndexOpen = m_IconIndexNormal;
                }
            }
            return m_IconIndexOpen;
        }
    }
    #endregion
    
    #region "           FileInfo type Information"
    
    /// <summary>
    /// Obtains information available from FileInfo.
    /// </summary>
    /// 
    private void FillDemandInfo()
    {
        if (m_IsDisk) {
            try {
                //See if this is a network drive
                //NoRoot = 1
                //Removable = 2
                //LocalDisk = 3
                //Network = 4
                //CD = 5
                //RAMDrive = 6
                System.Management.ManagementObject disk = new System.Management.ManagementObject("win32_logicaldisk.deviceid=\"" + m_Path.Substring(0, 2) + "\"");
                m_Length = (long)(UInt64)disk["Size"];
                if ((UInt32)disk["DriveType"] == 4) {
                    m_IsNetWorkDrive = true;
                }
            }
            catch (Exception) {
                //Disconnected Network Drives etc. will generate 
                //an error here, just assume that it is a network
                //drive
                m_IsNetWorkDrive = true;
            }
            finally {
                m_XtrInfo = true;
            }
        }
        else if (!m_IsDisk & m_IsFileSystem & !m_IsFolder) {
            //in this case, it's a file
            if (File.Exists(m_Path)) {
                FileInfo fi = new FileInfo(m_Path);
                m_LastWriteTime = fi.LastWriteTime;
                m_LastAccessTime = fi.LastAccessTime;
                m_CreationTime = fi.CreationTime;
                m_Length = fi.Length;
                m_XtrInfo = true;
            }
        }
        else {
            if (m_IsFileSystem & m_IsFolder) {
                if (Directory.Exists(m_Path)) {
                    DirectoryInfo di = new DirectoryInfo(m_Path);
                    m_LastWriteTime = di.LastWriteTime;
                    m_LastAccessTime = di.LastAccessTime;
                    m_CreationTime = di.CreationTime;
                    m_XtrInfo = true;
                }
            }
        }
    }
    
    public DateTime LastWriteTime {
        get {
            if (!m_XtrInfo) {
                FillDemandInfo();
            }
            return m_LastWriteTime;
        }
    }
    public DateTime LastAccessTime {
        get {
            if (!m_XtrInfo) {
                FillDemandInfo();
            }
            return m_LastAccessTime;
        }
    }
    public DateTime CreationTime {
        get {
            if (!m_XtrInfo) {
                FillDemandInfo();
            }
            return m_CreationTime;
        }
    }
    public long Length {
        get {
            if (!m_XtrInfo) {
                FillDemandInfo();
            }
            return m_Length;
        }
    }
    public bool IsNetworkDrive {
        get {
            if (!m_XtrInfo) {
                FillDemandInfo();
            }
            return m_IsNetWorkDrive;
        }
    }
    #endregion
    
    #region "           cPidl information"
    public cPidl clsPidl {
        get {
            if ((m_cPidl == null)) {
                m_cPidl = new cPidl(m_Pidl);
            }
            return m_cPidl;
        }
    }
    #endregion
    
    #region "       IsReadOnly and IsSystem"
    ///<Summary>The IsReadOnly attribute causes an annoying access to any floppy drives
    /// on the system. To postpone this (or avoid, depending on user action),
    /// the attribute is only queried when asked for</Summary>
    public bool IsReadOnly {
        get {
            if (m_IsReadOnlySetup) {
                return m_IsReadOnly;
            }
            else {
                ShellDll.SHFILEINFO shfi = new ShellDll.SHFILEINFO();
                shfi.dwAttributes = (int)ShellDll.SFGAO.RDONLY;
                int dwflag = (int)(ShellDll.SHGFI.PIDL | ShellDll.SHGFI.ATTRIBUTES | ShellDll.SHGFI.ATTR_SPECIFIED);
                int dwAttr = 0;
                IntPtr H = ShellDll.SHGetFileInfo(m_Pidl, dwAttr, ref shfi, ShellDll.cbFileInfo, dwflag);
                if (H.ToInt32() != ShellDll.NOERROR && H.ToInt32() != 1) {
                    Marshal.ThrowExceptionForHR(H.ToInt32());
                }
                m_IsReadOnly = ((shfi.dwAttributes & (int)ShellDll.SFGAO.RDONLY) != 0);
                //If m_IsReadOnly Then Debug.WriteLine("IsReadOnly -- " & m_Path)
                m_IsReadOnlySetup = true;
                return m_IsReadOnly;
            }
        }
        //If Not m_XtrInfo Then
        //    FillDemandInfo()
        //End If
        //Return m_Attributes And FileAttributes.ReadOnly = FileAttributes.ReadOnly
    }
    ///<Summary>The IsSystem attribute is seldom used, but required by DragDrop operations.
    /// Since there is no way of getting ONLY the System attribute without getting
    /// the RO attribute (which forces a reference to the floppy drive), we pay
    /// the price of getting its own File/DirectoryInfo for this purpose alone.
    ///</Summary>
    public bool IsSystem {
        get {
            //true once we have gotten this attr
            //the value of this attr once we have it
            if (!static_IsSystem_HaveSysInfo) {
                try {
                    static_IsSystem_m_IsSystem = (File.GetAttributes(m_Path) & FileAttributes.System) == FileAttributes.System;
                    static_IsSystem_HaveSysInfo = true;
                }
                catch (Exception) {
                    static_IsSystem_HaveSysInfo = true;
                }
            }
			Debug.WriteLine("In IsSystem -- Path = " + m_Path + " IsSystem = " + static_IsSystem_m_IsSystem);
            return static_IsSystem_m_IsSystem;
        }
    }
    static bool static_IsSystem_m_IsSystem = false;
    static bool static_IsSystem_HaveSysInfo = false;
    
    #endregion
    
    #endregion
    
    #endregion
    
    #region "   Public Methods"
    
    #region "       Shared Public Methods"
    
    #region "       GetDeskTop"
    /// <summary>
    /// If not initialized, then build DesktopBase
    /// once done, or if initialized already,
    /// </summary>
    ///<returns>The DesktopBase ShellItem representing the desktop</returns>
    /// 
    public static ShellItem GetDeskTop()
    {
        if ((DesktopBase == null)) {
            DesktopBase = new ShellItem();
        }
        return DesktopBase;
    }
    #endregion
    
    #region "       IsAncestorOf"
    ///<Summary>IsAncestorOf returns True if ShellItem ancestor is an ancestor of ShellItem current
    /// if OS is Win2K or above, uses the ILIsParent API, otherwise uses the
    /// cPidl function StartsWith.  This is necessary since ILIsParent in only available
    /// in Win2K or above systems AND StartsWith fails on some folders on XP systems (most
    /// obviously some Network Folder Shortcuts, but also Control Panel. Note, StartsWith
    /// always works on systems prior to XP.
    /// NOTE: if ancestor and current reference the same Item, both
    /// methods return True</Summary>
    public static bool IsAncestorOf(ShellItem ancestor, ShellItem current, [System.Runtime.InteropServices.OptionalAttribute, System.Runtime.InteropServices.DefaultParameterValueAttribute(false)]  // ERROR: Optional parameters aren't supported in C#
bool fParent)
    {
        return IsAncestorOf(ancestor.PIDL, current.PIDL, fParent);
    }
    ///<Summary> Compares a candidate Ancestor PIDL with a Child PIDL and
    /// returns True if Ancestor is an ancestor of the child.
    /// if fParent is True, then only return True if Ancestor is the immediate
    /// parent of the Child</Summary>
    public static bool IsAncestorOf(IntPtr AncestorPidl, IntPtr ChildPidl, [System.Runtime.InteropServices.OptionalAttribute, System.Runtime.InteropServices.DefaultParameterValueAttribute(false)]  // ERROR: Optional parameters aren't supported in C#
bool fParent)
    {
        bool functionReturnValue = false;
        if (ShellDll.Is2KOrAbove()) {
            return ShellDll.ILIsParent(AncestorPidl, ChildPidl, fParent);
        }
        else {
            cPidl Child = new cPidl(ChildPidl);
            cPidl Ancestor = new cPidl(AncestorPidl);
            functionReturnValue = Child.StartsWith(Ancestor);
            if (!functionReturnValue) return functionReturnValue;
 
            if (fParent) {
                // check for immediate ancestor, if desired
                object[] oAncBytes = Ancestor.Decompose();
                object[] oChildBytes = Child.Decompose();
                if (oAncBytes.Length != (oChildBytes.Length - 1)) {
                    functionReturnValue = false;
                }
            }
        }
        return functionReturnValue;
    }
    #endregion
    
    #region "      AllFolderWalk"
    ///<Summary>The WalkAllCallBack delegate defines the signature of 
    /// the routine to be passed to DirWalker
    /// Usage:  dim d1 as new CshItem.WalkAllCallBack(addressof yourroutine)
    ///   Callback function receives a ShellItem for each file and Directory in
    ///   Starting Directory and each sub-directory of this directory and
    ///   each sub-dir of each sub-dir ....
    ///</Summary>
    public delegate bool WalkAllCallBack(ShellItem info, int UserLevel, int Tag);
    ///<Summary>
    /// AllFolderWalk recursively walks down directories from cStart, calling its
    ///   callback routine, WalkAllCallBack, for each Directory and File encountered, including those in
    ///   cStart.  UserLevel is incremented by 1 for each level of dirs that DirWalker
    ///  recurses thru.  Tag in an Integer that is simply passed, unmodified to the 
    ///  callback, with each ShellItem encountered, both File and Directory CShItems.
    /// </Summary>
    /// <param name="cStart"></param>
    /// <param name="cback"></param>
    /// <param name="UserLevel"></param>
    /// <param name="Tag"></param>
    /// 
    public static bool AllFolderWalk(ShellItem cStart, WalkAllCallBack cback, int UserLevel, int Tag)
    {
        if ((cStart != null) && cStart.IsFolder) {
            //ShellItem cItem = default(ShellItem);
            //first processes all files in this directory
            foreach (var obj in cStart.GetFiles()) {
				ShellItem cItem = obj as ShellItem;
                if (!cback(cItem, UserLevel, Tag)) {
                        //user said stop
                    return false;
                }
            }
            //then process all dirs in this directory, recursively
            foreach (var obj in cStart.GetDirectories(true)) {
				ShellItem cItem = obj as ShellItem;
                if (!cback(cItem, UserLevel + 1, Tag)) {
                        //user said stop
                    return false;
                }
                else {
                    if (!AllFolderWalk(cItem, cback, UserLevel + 1, Tag)) {
                        return false;
                    }
                }
            }
            return true;
        }
        else {
            //Invalid call
            throw new ApplicationException("AllFolderWalk -- Invalid Start Directory");
        }
    }
    #endregion
    
    #endregion
    
    #region "       Public Instance Methods"
    
    #region "           Equals"
    public bool Equals(ShellItem other)
    {
        return this.Path.Equals(other.Path);
    }
    #endregion
    
    #region "       GetDirectories"
    /// <summary>
    /// Returns the Directories of this sub-folder as an ArrayList of CShitems
    /// </summary>
    /// <param name="doRefresh">Optional, default=True, Refresh the directories</param>
    /// <returns>An ArrayList of CShItems. May return an empty ArrayList if there are none.</returns>
    /// <remarks>revised to alway return an up-to-date list unless 
    /// specifically instructed not to (useful in constructs like:
    /// if CSI.RefreshDirectories then
    ///     Dirs = CSI.GetDirectories(False)  ' just did a Refresh </remarks>
    public ArrayList GetDirectories([System.Runtime.InteropServices.OptionalAttribute, System.Runtime.InteropServices.DefaultParameterValueAttribute(true)]  // ERROR: Optional parameters aren't supported in C#
bool doRefresh)
    {
        if (m_IsFolder) {
            if (doRefresh) {
                    // return an up-to-date List
                RefreshDirectories();
            }
            else if (m_Directories == null) {
                RefreshDirectories();
            }
            return m_Directories;
        }
        else {
            //if it is not a Folder, then return empty arraylist
            return new ArrayList();
        }
    }
    
    #endregion
    
    #region "       GetFiles"
    /// <summary>
    /// Returns the Files of this sub-folder as an
    ///   ArrayList of CShitems
    /// Note: we do not keep the arraylist of files, Generate it each time
    /// </summary>
    /// <returns>An ArrayList of CShItems. May return an empty ArrayList if there are none.</returns>
    /// 
    public ArrayList GetFiles()
    {
        if (m_IsFolder) {
            return GetContents(ShellDll.SHCONTF.NONFOLDERS | ShellDll.SHCONTF.INCLUDEHIDDEN, false);
        }
        else {
            return new ArrayList();
        }
    }
    
    /// <summary>
    /// Returns the Files of this sub-folder, filtered by a filtering string, as an
    ///   ArrayList of CShitems
    /// Note: we do not keep the arraylist of files, Generate it each time
    /// </summary>
    /// <param name="Filter">A filter string (for example: *.Doc)</param>
    /// <returns>An ArrayList of CShItems. May return an empty ArrayList if there are none.</returns>
    /// 
    public ArrayList GetFiles(string Filter)
    {
        if (m_IsFolder) {
            ArrayList dummy = new ArrayList();
            string[] fileentries = null;
            fileentries = Directory.GetFiles(m_Path, Filter);
            //string vFile = null;
            foreach (var obj in fileentries) {
				string vFile = obj as string;
                dummy.Add(new ShellItem(vFile));
            }
            return dummy;
        }
        else {
            return new ArrayList();
        }
    }
    #endregion
    
    #region "       GetItems"
    /// <summary>
    /// Returns the Directories and Files of this sub-folder as an
    ///   ArrayList of CShitems
    /// Note: we do not keep the arraylist of files, Generate it each time
    /// </summary>
    /// <returns>An ArrayList of CShItems. May return an empty ArrayList if there are none.</returns>
    public ArrayList GetItems()
    {
        ArrayList rVal = new ArrayList();
        if (m_IsFolder) {
            rVal.AddRange(this.GetDirectories(true));
            rVal.AddRange(this.GetContents(ShellDll.SHCONTF.NONFOLDERS | ShellDll.SHCONTF.INCLUDEHIDDEN, false));
            rVal.Sort();
            return rVal;
        }
        else {
            return rVal;
        }
    }
    #endregion
    
    #region "       GetFileName"
    ///<Summary>GetFileName returns the Full file name of this item.
    ///  Specifically, for a link file (xxx.txt.lnk for example) the
    ///  DisplayName property will return xxx.txt, this method will
    ///  return xxx.txt.lnk.  In most cases this is equivalent of
    ///  System.IO.Path.GetFileName(m_Path).  However, some m_Paths
    ///  actually are GUIDs.  In that case, this routine returns the
    ///  DisplayName</Summary>
    public string GetFileName()
    {
        if (m_Path.StartsWith("::{")) {
            //Path is really a GUID
            return this.DisplayName;
        }
        else {
            if (m_IsDisk) {
                return m_Path.Substring(0, 1);
            }
            else {
                return System.IO.Path.GetFileName(m_Path);
            }
        }
    }
    #endregion
    
    #region "       ReFreshDirectories"
    ///<Summary> A lower cost way of Refreshing the Directories of this ShellItem</Summary>
    ///<returns> Returns True if there were any changes</returns>
    public bool RefreshDirectories()
    {
        bool functionReturnValue = false;
        functionReturnValue = false;
        //value unless there were changes
        if (m_IsFolder) {
            //if not a folder, then return false
            ArrayList InvalidDirs = new ArrayList();
            //holds CShItems of not found dirs
            if ((m_Directories == null)) {
                m_Directories = GetContents(ShellDll.SHCONTF.FOLDERS | ShellDll.SHCONTF.INCLUDEHIDDEN, false);
                    //changed from unexamined to examined
                functionReturnValue = true;
            }
            else {
                //Get relative PIDLs from current directory items
                ArrayList curPidls = GetContents(ShellDll.SHCONTF.FOLDERS | ShellDll.SHCONTF.INCLUDEHIDDEN, true);
                //IntPtr iptr = default(IntPtr);
                //used below
                if (curPidls.Count < 1) {
                    if (m_Directories.Count > 0) {
                        m_Directories = new ArrayList();
                        //nothing there anymore
                            //Changed from had some to have none
                        functionReturnValue = true;
                    }
                    else {
                        //Empty before, Empty now, do nothing -- just a logic marker
                    }
                }
                else {
                    //still has some. Are they the same?
                    if (m_Directories.Count < 1) {
                        //didn't have any before, so different
                        m_Directories = GetContents(ShellDll.SHCONTF.FOLDERS | ShellDll.SHCONTF.INCLUDEHIDDEN, false);
                            //changed from had none to have some
                        functionReturnValue = true;
                    }
                    else {
                        //some before, some now. Same?  This is the complicated part
                        //Firstly, build ArrayLists of Relative Pidls
                        ArrayList compList = new ArrayList(curPidls);
                        //Since we are only comparing relative PIDLs, build a list of 
                        // the relative PIDLs of the old content -- saving repeated building
                        int iOld = 0;
                        IntPtr[] OldRel = new IntPtr[m_Directories.Count];
                        for (iOld = 0; iOld <= m_Directories.Count - 1; iOld++) {
                            //GetLastID returns a ptr into an EXISTING IDLIST -- never release that ptr
                            // and never release the EXISTING IDLIST before thru with OldRel
                            OldRel[iOld] = GetLastID(((ShellItem)m_Directories[iOld]).PIDL);
                        }
                        int iNew = 0;
                        for (iOld = 0; iOld <= m_Directories.Count - 1; iOld++) {
                            for (iNew = 0; iNew <= compList.Count - 1; iNew++) {
                                if (IsEqual((IntPtr)compList[iNew], OldRel[iOld])) {
                                    compList.RemoveAt(iNew);
                                    //Match, don't look at this one again
                                        //content item exists in both
                                    goto NXTOLD;
                                }
                            }
                            //falling thru here means couldn't find iOld entry
                            InvalidDirs.Add(m_Directories[iOld]);
                            //save off the unmatched ShellItem
                            functionReturnValue = true;
						NXTOLD: functionReturnValue = !!functionReturnValue;
                        }
                        //any not found should be removed from m_Directories
                        //ShellItem csi = default(ShellItem);
                        foreach (var obj in InvalidDirs) {
							ShellItem csi = obj as ShellItem;
                            m_Directories.Remove(csi);
                        }
                        //anything remaining in compList is a new entry
                        if (compList.Count > 0) {
                            functionReturnValue = true;
                            foreach (var obj in compList) {
								IntPtr iptr = (IntPtr)obj;
                                //these are relative PIDLs
                                m_Directories.Add(new ShellItem(m_Folder, iptr, this));
                            }
                        }
                        if (functionReturnValue) {
                            //something added or removed, resort
                            m_Directories.Sort();
                        }
                    }
                    //we obtained some new relative PIDLs in curPidls, so free them
                    foreach (var obj in curPidls) {
						IntPtr iptr = (IntPtr)obj;
						Marshal.FreeCoTaskMem(iptr);
                    }
                    //end of content comparison
                }
                //end of IsNothing test
            }
        }
            //end of IsFolder test
        return functionReturnValue;
    }
    
    #endregion
    
    #region "       ToString"
    /// <summary>
    /// Returns the DisplayName as the normal ToString value
    /// </summary>
    /// 
    public override string ToString()
    {
        return m_DisplayName;
    }
    #endregion
    
    #region "       Debug Dumper"
    /// <summary>
    /// Summary of DebugDump.
    /// </summary>
    /// 
    public void DebugDump()
    {
        Debug.WriteLine("DisplayName = " + m_DisplayName);
        Debug.WriteLine("PIDL        = " + m_Pidl.ToString());
        Debug.WriteLine("\tPath        = " + m_Path);
		Debug.WriteLine("\tTypeName    = " + this.TypeName);
		Debug.WriteLine("\tiIconNormal = " + m_IconIndexNormal);
		Debug.WriteLine("\tiIconSelect = " + m_IconIndexOpen);
		Debug.WriteLine("\tIsBrowsable = " + m_IsBrowsable);
		Debug.WriteLine("\tIsFileSystem= " + m_IsFileSystem);
		Debug.WriteLine("\tIsFolder    = " + m_IsFolder);
		Debug.WriteLine("\tIsLink    = " + m_IsLink);
		Debug.WriteLine("\tIsDropTarget = " + m_IsDropTarget);
		Debug.WriteLine("\tIsReadOnly   = " + this.IsReadOnly);
		Debug.WriteLine("\tCanCopy = " + this.CanCopy);
		Debug.WriteLine("\tCanLink = " + this.CanLink);
		Debug.WriteLine("\tCanMove = " + this.CanMove);
		Debug.WriteLine("\tCanDelete = " + this.CanDelete);
        if (m_IsFolder) {
            if ((m_Directories != null)) {
                Debug.WriteLine("\tDirectory Count = " + m_Directories.Count);
            }
            else {
                Debug.WriteLine("\tDirectory Count Not yet set");
            }
        }
    }
    #endregion
    
    #region "       GetDropTargetOf"
    public IDropTarget GetDropTargetOf(Control tn)
    {
		//if ((m_Folder == null)) return null; 
		//IntPtr[] apidl = new IntPtr[1];
		//int HR = 0;
        IDropTarget theInterface = null;
		//IntPtr tnH = tn.Handle;
		//HR = m_Folder.CreateViewObject(tnH, ref ShellDll.IID_IDropTarget, ref theInterface);
		//if (HR != 0) {
		//    Marshal.ThrowExceptionForHR(HR);
		//}
        return theInterface;
    }
    #endregion
    
    #endregion
    
    #endregion
    
    #region "   Private Instance Methods"
    
    #region "       GetContents"
    ///<Summary>
    /// Returns the requested Items of this Folder as an ArrayList of CShitems
    ///  unless the IntPtrOnly flag is set.  If IntPtrOnly is True, return an
    ///  ArrayList of PIDLs.
    ///</Summary>
    /// <param name="flags">A set of one or more ShellDll.SHCONTF flags indicating which items to return</param>
    /// <param name="IntPtrOnly">True to suppress generation of CShItems, returning only an
    ///  ArrayList of IntPtrs to RELATIVE PIDLs for the contents of this Folder</param>
    private ArrayList GetContents(ShellDll.SHCONTF flags, [System.Runtime.InteropServices.OptionalAttribute, System.Runtime.InteropServices.DefaultParameterValueAttribute(false)]  // ERROR: Optional parameters aren't supported in C#
bool IntPtrOnly)
    {
        ArrayList rVal = new ArrayList();
        int HR = 0;
		ShellDll.IEnumIDList IEnum = null;
        HR = m_Folder.EnumObjects(0, flags, ref IEnum);
        if (HR == ShellDll.NOERROR) {
            IntPtr item = IntPtr.Zero;
            int itemCnt = 0;
            HR = IEnum.GetNext(1, ref item, ref itemCnt);
            if (HR == ShellDll.NOERROR) {
                while (itemCnt > 0 && !item.Equals(IntPtr.Zero)) {
                    //there is no setting to exclude folders from enumeration,
                    // just one to include non-folders
                    // so we have to screen the results to return only
                    //  non-folders if folders are not wanted
                    if ((flags & ShellDll.SHCONTF.FOLDERS) == 0) {
                        //don't want folders. see if this is one
                        ShellDll.SFGAO attrFlag = default(ShellDll.SFGAO);
                        attrFlag = attrFlag | ShellDll.SFGAO.FOLDER | ShellDll.SFGAO.STREAM;
                        //Note: for GetAttributesOf, we must provide an array, in all cases with 1 element
                        IntPtr[] aPidl = new IntPtr[1];
                        aPidl[0] = item;
                        m_Folder.GetAttributesOf(1, aPidl, ref attrFlag);
                        if (!XPorAbove) {
                            if ((attrFlag & ShellDll.SFGAO.FOLDER) != 0) {
                                //Don't need it
                                goto SKIPONE;
                            }
                        }
                        else {
                            //XP or above
                            if ((attrFlag & ShellDll.SFGAO.FOLDER) != 0 && (attrFlag & ShellDll.SFGAO.STREAM) == 0) {
                                goto SKIPONE;
                            }
                        }
                    }
                    if (IntPtrOnly) {
                        //just relative pidls for fast look, no CShITem overhead
                            //caller must free
                        rVal.Add(PIDLClone(item));
                    }
                    else {
                        rVal.Add(new ShellItem(m_Folder, item, this));
                    }
                    SKIPONE:
                    Marshal.FreeCoTaskMem(item);
                    //if New kept it, it kept a copy
                    item = IntPtr.Zero;
                    itemCnt = 0;
                    // Application.DoEvents()
                    HR = IEnum.GetNext(1, ref item, ref itemCnt);
                }
            }
            else {
                    //1 means no more
                if (HR != 1) goto HRError; 
            }
        }
        else {
            goto HRError;
        }
        NORMAL:
        //Normal Exit
        if ((IEnum != null)) {
            Marshal.ReleaseComObject(IEnum);
        }
        rVal.TrimToSize();
        return rVal;
        HRError:
        
        // Error Exit for all Com errors
        //not ready disks will return the following error
        //If HR = &HFFFFFFFF800704C7 Then
        //    GoTo NORMAL
        //ElseIf HR = &HFFFFFFFF80070015 Then
        //    GoTo NORMAL
        //    'unavailable net resources will return these
        //ElseIf HR = &HFFFFFFFF80040E96 Or HR = &HFFFFFFFF80040E19 Then
        //    GoTo NORMAL
        //ElseIf HR = &HFFFFFFFF80004001 Then 'Certain "Not Implemented" features will return this
        //    GoTo NORMAL
        //ElseIf HR = &HFFFFFFFF80004005 Then
        //    GoTo NORMAL
        //ElseIf HR = &HFFFFFFFF800704C6 Then
        //    GoTo NORMAL
        //End If
        if ((IEnum != null)) Marshal.ReleaseComObject(IEnum); 
        //#If Debug Then
        //        Marshal.ThrowExceptionForHR(HR)
        //#End If
        rVal = new ArrayList();
        //sometimes it is a non-fatal error,ignored
        goto NORMAL;
    }
    #endregion
    
    #region "       Really nasty Pidl manipulation"
    
    /// <Summary>
    /// Get FileSize in bytes of the first (possibly only)
    ///  SHItem in an ID list.  Note: the full Size of
    ///   the item is the sum of the sizes of all SHItems
    ///   in the list!!
    /// </Summary>
    /// <param name="pidl"></param>
    /// 
    private static int ItemIDSize(IntPtr pidl)
    {
        if (!pidl.Equals(IntPtr.Zero)) {
            byte[] b = new byte[2];
            Marshal.Copy(pidl, b, 0, 2);
            return b[1] * 256 + b[0];
        }
        else {
            return 0;
        }
    }
    
    /// <summary>
    /// computes the actual Size of the ItemIDList pointed to by pidl
    /// </summary>
    /// <param name="pidl">The pidl pointing to an ItemIDList</param>
    ///<returns> Returns actual Size of the ItemIDList, less the terminating nulnul</returns> 
    public static int ItemIDListSize(IntPtr pidl)
    {
        if (!pidl.Equals(IntPtr.Zero)) {
            int i = ItemIDSize(pidl);
            int b = Marshal.ReadByte(pidl, i) + (Marshal.ReadByte(pidl, i + 1) * 256);
            while (b > 0) {
                i += b;
                b = Marshal.ReadByte(pidl, i) + (Marshal.ReadByte(pidl, i + 1) * 256);
            }
            return i;
        }
        else {
            return 0;
        }
    }
    /// <summary>
    /// Counts the total number of SHItems in input pidl
    /// </summary>
    /// <param name="pidl">The pidl to obtain the count for</param>
    /// <returns> Returns the count of SHItems pointed to by pidl</returns> 
    public static int PidlCount(IntPtr pidl)
    {
        if (!pidl.Equals(IntPtr.Zero)) {
            int cnt = 0;
            int i = 0;
            int b = Marshal.ReadByte(pidl, i) + (Marshal.ReadByte(pidl, i + 1) * 256);
            while (b > 0) {
                cnt += 1;
                i += b;
                b = Marshal.ReadByte(pidl, i) + (Marshal.ReadByte(pidl, i + 1) * 256);
            }
            return cnt;
        }
        else {
            return 0;
        }
    }
    
    ///<Summary>GetLastId -- returns a pointer to the last ITEMID in a valid
    /// ITEMIDLIST. Returned pointer SHOULD NOT be released since it
    /// points to place within the original PIDL</Summary>
    ///<returns>IntPtr pointing to last ITEMID in ITEMIDLIST structure,
    /// Returns IntPtr.Zero if given a null pointer.
    /// If given a pointer to the Desktop, will return same pointer.</returns>
    ///<remarks>This is what the API ILFindLastID does, however IL... 
    /// functions are not supported before Win2K.</remarks>
    public static IntPtr GetLastID(IntPtr pidl)
    {
        if (!pidl.Equals(IntPtr.Zero)) {
            int prev = 0;
            int i = 0;
            int b = Marshal.ReadByte(pidl, i) + (Marshal.ReadByte(pidl, i + 1) * 256);
            while (b > 0) {
                prev = i;
                i += b;
                b = Marshal.ReadByte(pidl, i) + (Marshal.ReadByte(pidl, i + 1) * 256);
            }
            return new IntPtr(pidl.ToInt32() + prev);
        }
        else {
            return IntPtr.Zero;
        }
    }
    
    public static IntPtr[] DecomposePIDL(IntPtr pidl)
    {
        int lim = ItemIDListSize(pidl);
        IntPtr[] PIDLs = new IntPtr[PidlCount(pidl)];
        int i = 0;
        int curB = 0;
        int offSet = 0;
        while (curB < lim) {
            IntPtr thisPtr = new IntPtr(pidl.ToInt32() + curB);
            offSet = Marshal.ReadByte(thisPtr) + (Marshal.ReadByte(thisPtr, 1) * 256);
            PIDLs[i] = Marshal.AllocCoTaskMem(offSet + 2);
            byte[] b = new byte[offSet + 2];
            Marshal.Copy(thisPtr, b, 0, offSet);
            b[offSet] = 0;
            b[offSet + 1] = 0;
            Marshal.Copy(b, 0, PIDLs[i], offSet + 2);
            //DumpPidl(PIDLs(i))
            curB += offSet;
            i += 1;
        }
        return PIDLs;
    }
    
    private static IntPtr PIDLClone(IntPtr pidl)
    {
        IntPtr functionReturnValue = default(IntPtr);
        int cb = ItemIDListSize(pidl);
        byte[] b = new byte[cb + 2];
        Marshal.Copy(pidl, b, 0, cb);
        //not including terminating nulnul
        b[cb] = 0;
        b[cb + 1] = 0;
        //force to nulnul
        functionReturnValue = Marshal.AllocCoTaskMem(cb + 2);
        Marshal.Copy(b, 0, functionReturnValue, cb + 2);
        return functionReturnValue;
    }
    
    public static bool IsEqual(IntPtr Pidl1, IntPtr Pidl2)
    {
        if (Win2KOrAbove) {
            return ShellDll.ILIsEqual(Pidl1, Pidl2);
        }
        else {
            //do hard way, may not work for some folders on XP
            
            int cb1 = 0;
            int cb2 = 0;
            cb1 = ItemIDListSize(Pidl1);
            cb2 = ItemIDListSize(Pidl2);
            if (cb1 != cb2) return false; 
            int lim32 = cb1 / 4;
            
            int i = 0;
            for (i = 0; i <= lim32 - 1; i++) {
                if (Marshal.ReadInt32(Pidl1, i) != Marshal.ReadInt32(Pidl2, i)) {
                    return false;
                }
            }
            int limB = cb1 % 4;
            int offset = lim32 * 4;
            for (i = 0; i <= limB - 1; i++) {
                if (Marshal.ReadByte(Pidl1, offset + i) != Marshal.ReadByte(Pidl2, offset + i)) {
                    return false;
                }
            }
                //made it to here, so they are equal
            return true;
        }
    }
    
    /// <summary>
    /// Concatenates the contents of two pidls into a new Pidl (ended by 00)
    /// allocating CoTaskMem to hold the result,
    /// placing the concatenation (followed by 00) into the
    /// allocated Memory,
    /// and returning an IntPtr pointing to the allocated mem
    /// </summary>
    /// <param name="pidl1">IntPtr to a well formed SHItemIDList or IntPtr.Zero</param>
    /// <param name="pidl2">IntPtr to a well formed SHItemIDList or IntPtr.Zero</param>
    /// <returns>Returns a ptr to an ItemIDList containing the 
    ///   concatenation of the two (followed by the req 2 zeros
    ///   Caller must Free this pidl when done with it</returns> 
    public static IntPtr concatPidls(IntPtr pidl1, IntPtr pidl2)
    {
        int cb1 = 0;
        int cb2 = 0;
        cb1 = ItemIDListSize(pidl1);
        cb2 = ItemIDListSize(pidl2);
        int rawCnt = cb1 + cb2;
        if ((rawCnt) > 0) {
            byte[] b = new byte[rawCnt + 2];
            if (cb1 > 0) {
                Marshal.Copy(pidl1, b, 0, cb1);
            }
            if (cb2 > 0) {
                Marshal.Copy(pidl2, b, cb1, cb2);
            }
            IntPtr rVal = Marshal.AllocCoTaskMem(cb1 + cb2 + 2);
            b[rawCnt] = 0;
            b[rawCnt + 1] = 0;
            Marshal.Copy(b, 0, rVal, rawCnt + 2);
            return rVal;
        }
        else {
            return IntPtr.Zero;
        }
    }
    
    /// <summary>
    /// Returns an ItemIDList with the last ItemID trimed off
    ///  This is necessary since I cannot get SHBindToParent to work 
    ///  It's purpose is to generate an ItemIDList for the Parent of a
    ///  Special Folder which can then be processed with DesktopBase.BindToObject,
    ///  yeilding a Folder for the parent of the Special Folder
    ///  It also creates and passes back a RELATIVE pidl for this item
    /// </summary>
    /// <param name="pidl">A pointer to a well formed ItemIDList. The PIDL to trim</param>
    /// <param name="relPidl">BYREF IntPtr which will point to a new relative pidl
    ///        containing the contents of the last ItemID in the ItemIDList
    ///        terminated by the required 2 nulls.</param>
    /// <returns> an ItemIDList with the last element removed.
    ///  Caller must Free this item when through with it
    ///  Also returns the new relative pidl in the 2nd parameter
    ///   Caller must Free this pidl as well, when through with it
    ///</returns>
    public static IntPtr TrimPidl(IntPtr pidl, ref IntPtr relPidl)
    {
        int cb = ItemIDListSize(pidl);
        byte[] b = new byte[cb + 2];
        Marshal.Copy(pidl, b, 0, cb);
        int prev = 0;
        int i = b[0] + (b[1] * 256);
        //Do While i < cb AndAlso b(i) <> 0
        while (i > 0 && i < cb) {
            //Changed code
            prev = i;
            i += b[i] + (b[i + 1] * 256);
        }
        if ((prev + 1) < cb) {
            //first set up the relative pidl
            b[cb] = 0;
            b[cb + 1] = 0;
            int cb1 = b[prev] + (b[prev + 1] * 256);
            relPidl = Marshal.AllocCoTaskMem(cb1 + 2);
            Marshal.Copy(b, prev, relPidl, cb1 + 2);
            b[prev] = 0;
            b[prev + 1] = 0;
            IntPtr rVal = Marshal.AllocCoTaskMem(prev + 2);
            Marshal.Copy(b, 0, rVal, prev + 2);
            return rVal;
        }
        else {
            return IntPtr.Zero;
        }
    }
    
    #region "   DumpPidl Routines"
    /// <summary>
    /// Dumps, to the Debug output, the contents of the mem block pointed to by
    /// a PIDL. Depends on the internal structure of a PIDL
    /// </summary>
    /// <param name="pidl">The IntPtr(a PIDL) pointing to the block to dump</param>
    /// 
    public static void DumpPidl(IntPtr pidl)
    {
        int cb = ItemIDListSize(pidl);
        Debug.WriteLine("PIDL " + pidl.ToString() + " contains " + cb + " bytes");
        if (cb > 0) {
            byte[] b = new byte[cb + 2];
            Marshal.Copy(pidl, b, 0, cb + 1);
            int pidlCnt = 1;
            int i = b[0] + (b[1] * 256);
            int curB = 0;
            while (i > 0) {
                Debug.Write("ItemID #" + pidlCnt + " Length = " + i);
                DumpHex(b, curB, curB + i - 1);
                pidlCnt += 1;
                curB += i;
                i = b[curB] + (b[curB + 1] * 256);
            }
        }
    }
    
    ///<Summary>Dump a portion or all of a Byte Array to Debug output</Summary>
    ///<param name = "b">A single dimension Byte Array</param>
    ///<param name = "sPos">Optional start index of area to dump (default = 0)</param>
    ///<param name = "epos">Optional last index position to dump (default = end of array)</param>
    ///<Remarks>
    ///</Remarks>
    public static void DumpHex(byte[] b, [System.Runtime.InteropServices.OptionalAttribute, System.Runtime.InteropServices.DefaultParameterValueAttribute(0)]  // ERROR: Optional parameters aren't supported in C#
int sPos, [System.Runtime.InteropServices.OptionalAttribute, System.Runtime.InteropServices.DefaultParameterValueAttribute(0)]  // ERROR: Optional parameters aren't supported in C#
int ePos)
    {
        if (ePos == 0) ePos = b.Length - 1; 
        int j = 0;
        int curB = sPos;
        string sTmp = null;
        char ch = '\0';
        StringBuilder SBH = new StringBuilder();
        StringBuilder SBT = new StringBuilder();
        for (j = 0; j <= ePos - sPos; j++) {
            if (j % 16 == 0) {
                Debug.WriteLine(SBH.ToString() + SBT.ToString());
                SBH = new StringBuilder();
                SBT = new StringBuilder("          ");
                SBH.Append(HexNum(j + sPos, 4) + "). ");
            }
            if (b[curB] < 16) {
				sTmp = "0" + Convert.ToString(b[curB], 16);
            }
            else {
                sTmp = Convert.ToString(b[curB], 16);
            }
            SBH.Append(sTmp);
            SBH.Append(" ");
            ch = (char)(b[curB]);
            if (char.IsControl(ch)) {
                SBT.Append(".");
            }
            else {
                SBT.Append(ch);
            }
            curB += 1;
        }
        
        int fill = (j) % 16;
        if (fill != 0) {
            SBH.Append(' ', 48 - (3 * ((j) % 16)));
        }
        Debug.WriteLine(SBH.ToString() + SBT.ToString());
	}
    
    public static string HexNum(int num, int nrChrs)
    {
		string h = Convert.ToString(num, 16);
        StringBuilder SB = new StringBuilder();
        int i = 0;
        for (i = 1; i <= nrChrs - h.Length; i++) {
            SB.Append("0");
        }
        SB.Append(h);
        return SB.ToString();
    }
    #endregion
    
    #endregion
    
    #endregion
    
    #region "   TagComparer Class"
    ///<Summary> It is sometimes useful to sort a list of TreeNodes,
    /// ListViewItems, or other objects in an order based on CShItems in their Tag
    /// use this Icomparer for that Sort</Summary>
    public class TagComparer : IComparer
    {
        public int Compare(object x, object y)
        {
			//ShellItem xTag = x.tag;
			//ShellItem yTag = y.tag;
			//return xTag.CompareTo(y.tag);
			return 0;
        }
    }
    #endregion
    
    #region "   cPidl Class"
    ///<Summary>cPidl class contains a Byte() representation of a PIDL and
    /// certain Methods and Properties for comparing one cPidl to another</Summary>
    public class cPidl : IEnumerable
    {
        
        #region "       Private Fields"
        byte[] m_bytes = null;
        //The local copy of the PIDL
        int m_ItemCount = 0;
        //the # of ItemIDs in this ItemIDList (PIDL)
        #endregion
        
        #region "       Constructor"
        public cPidl(IntPtr pidl)
        {
            int cb = ItemIDListSize(pidl);
            if (cb > 0) {
				m_bytes = new byte[cb + 2];
                Marshal.Copy(pidl, m_bytes, 0, cb);
				//DumpPidl(pidl)
            }
            else {
				m_bytes = new byte[2]; //This is the DeskTop (we hope)
            }
            //ensure nulnul
            m_bytes[m_bytes.Length - 2]= 0;
            m_bytes[m_bytes.Length - 1] = 0;
            m_ItemCount = PidlCount(pidl);
        }
        #endregion
        
        #region "       Public Properties"
        public byte[] PidlBytes {
            get { return m_bytes; }
        }
        
        public int Length {
            get { return m_bytes.Length; }
        }
        
        public int ItemCount {
            get { return m_ItemCount; }
        }
        
        #endregion
        
        #region "       Public Intstance Methods -- ToPIDL, Decompose, and IsEqual"
        
        ///<Summary> Copy the contents of a byte() containing a pidl to
        ///  CoTaskMemory, returning an IntPtr that points to that mem block
        /// Assumes that this cPidl is properly terminated, as all New 
        /// cPidls are.
        /// Caller must Free the returned IntPtr when done with mem block.
        ///</Summary>
        public IntPtr ToPIDL()
        {
            return BytesToPidl(m_bytes);
        }
        
        ///<Summary>Returns an object containing a byte() for each of this cPidl's
        /// ITEMIDs (individual PIDLS), in order such that obj(0) is
        /// a byte() containing the bytes of the first ITEMID, etc.
        /// Each ITEMID is properly terminated with a nulnul
        ///</Summary>
        public object[] Decompose()
        {
            object[] bArrays = new object[this.ItemCount];
			ICPidlEnumerator eByte = this.GetEnumerator() as ICPidlEnumerator;
            int i = 0;
            while (eByte.MoveNext()) {
                bArrays[i] = eByte.Current;
                i += 1;
            }
            return bArrays;
        }
        
        ///<Summary>Returns True if input cPidl's content exactly match the 
        /// contents of this instance</Summary>
        public bool IsEqual(cPidl other)
        {
            bool functionReturnValue = false;
            functionReturnValue = false;
            //assume not
            if (other.Length != this.Length) return functionReturnValue;
 
            byte[] ob = other.PidlBytes;
            int i = 0;
            for (i = 0; i <= this.Length - 1; i++) {
                //note: we look at nulnul also
                if (ob[i] != m_bytes[i]) return functionReturnValue;
 
            }
                //all equal on fall thru
            return true;
		}
        #endregion
        
        #region "       Public Shared Methods"
        
        #region "           JoinPidlBytes"
        ///<Summary> Join two byte arrays containing PIDLS, returning a 
        /// Byte() containing the resultant ITEMIDLIST. Both Byte() must
        /// be properly terminated (nulnul)
        /// Returns NOTHING if error
        /// </Summary>
        public static byte[] JoinPidlBytes(byte[] b1, byte[] b2)
        {
            if (IsValidPidl(b1) & IsValidPidl(b2)) {
                byte[] b = new byte[b1.Length + b2.Length - 2];
                //allow for leaving off first nulnul
                Array.Copy(b1, b, b1.Length - 2);
                Array.Copy(b2, 0, b, b1.Length - 2, b2.Length);
                if (IsValidPidl(b)) {
                    return b;
                }
                else {
                    return null;
                }
            }
            else {
                return null;
            }
        }
        #endregion
        
        #region "           BytesToPidl"
        ///<Summary> Copy the contents of a byte() containing a pidl to
        ///  CoTaskMemory, returning an IntPtr that points to that mem block
        /// Caller must free the IntPtr when done with it
        ///</Summary>
        public static IntPtr BytesToPidl(byte[] b)
        {
            IntPtr functionReturnValue = default(IntPtr);
            functionReturnValue = IntPtr.Zero;
            //assume failure
            if (IsValidPidl(b)) {
                int bLen = b.Length;
                functionReturnValue = Marshal.AllocCoTaskMem(bLen);
                if (functionReturnValue.Equals(IntPtr.Zero)) return functionReturnValue;
 
                //another bad error
                Marshal.Copy(b, 0, functionReturnValue, bLen);
            }
            return functionReturnValue;
        }
        #endregion
        
        #region "           StartsWith"
        ///<Summary>returns True if the beginning of pidlA matches PidlB exactly for pidlB's entire length</Summary>
        public static bool StartsWith(IntPtr pidlA, IntPtr pidlB)
        {
            return cPidl.StartsWith(new cPidl(pidlA), new cPidl(pidlB));
        }
        
        ///<Summary>returns True if the beginning of A matches B exactly for B's entire length</Summary>
        public static bool StartsWith(cPidl A, cPidl B)
        {
            return A.StartsWith(B);
        }
        
        ///<Summary>Returns true if the CPidl input parameter exactly matches the
        /// beginning of this instance of CPidl</Summary>
        public bool StartsWith(cPidl cp)
        {
            byte[] b = cp.PidlBytes;
            if (b.Length > m_bytes.Length) return false; 
            //input is longer
            int i = 0;
            for (i = 0; i <= b.Length - 3; i++) {
                //allow for nulnul at end of cp.PidlBytes
                if (b[i] != m_bytes[i]) return false; 
            }
            return true;
        }
        #endregion
        
        #endregion
        
        #region "       GetEnumerator"
        public System.Collections.IEnumerator GetEnumerator()
        {
            return new ICPidlEnumerator(m_bytes);
        }
        #endregion
        
        #region "       PIDL enumerator Class"
        private class ICPidlEnumerator : IEnumerator
        {
            
            private int m_sPos;
            //the first index in the current PIDL
            private int m_ePos;
            //the last index in the current PIDL
            private byte[] m_bytes;
            //the local copy of the PIDL
            private bool m_NotEmpty = false;
            //the desktop PIDL is zero length
            
            public ICPidlEnumerator(byte[] b)
            {
                m_bytes = b;
                if (b.Length > 0) m_NotEmpty = true; 
                m_sPos = -1;
                m_ePos = -1;
            }
            
            public object Current {
                get {
                    if (m_sPos < 0) throw new InvalidOperationException("ICPidlEnumerator --- attempt to get Current with invalidated list"); 
                    byte[] b = new byte[(m_ePos - m_sPos) + 3];
                    //room for nulnul
                    Array.Copy(m_bytes, m_sPos, b, 0, b.Length - 2);
                    b[b.Length - 2] = 0;
                    b[b.Length - 1] = 0;
                    //add nulnul
                    return b;
                }
            }
            
            public bool MoveNext()
            {
                if (m_NotEmpty) {
                    if (m_sPos < 0) {
                        m_sPos = 0;
                        m_ePos = -1;
                    }
                    else {
                        m_sPos = m_ePos + 1;
                    }
                    if (m_bytes.Length < m_sPos + 1) throw new InvalidCastException("Malformed PIDL"); 
                    int cb = m_bytes[m_sPos] + m_bytes[m_sPos + 1] * 256;
                    if (cb == 0) {
                            //have passed all back
                        return false;
                    }
                    else {
                        m_ePos += cb;
                    }
                }
                else {
                    m_sPos = 0;
                    m_ePos = 0;
                        //in this case, we have exhausted the list of 0 ITEMIDs
                    return false;
                }
                return true;
            }
            
            public void Reset()
            {
                m_sPos = -1;
                m_ePos = -1;
            }
        }
        #endregion
        
    }
    #endregion
    
}
