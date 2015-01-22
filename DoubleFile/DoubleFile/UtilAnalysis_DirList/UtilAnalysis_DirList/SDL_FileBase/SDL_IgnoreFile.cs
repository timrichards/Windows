
namespace SearchDirLists
{
    class SDL_IgnoreFile : SDL_FileBase { internal SDL_IgnoreFile(string strFile = null) : base(ksIgnoreListHeader, ksFileExt_Ignore, "ignore") { m_strFileNotDialog = strFile; } }
}
