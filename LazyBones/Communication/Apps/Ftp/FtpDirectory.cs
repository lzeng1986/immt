using System.IO;
using System.Linq;

namespace LazyBones.Communication.Apps.Ftp
{
    class FtpDirectory
    {
        string currentWorkingDirectory = "\\";

        public string CurrentWorkingDirectory
        {
            get { return currentWorkingDirectory; }
        }

        string root;

        public FtpDirectory(string root)
        {
            this.root = root;
        }
        public string GetPath(string dir)
        {
            if (string.IsNullOrEmpty(dir))
                return currentWorkingDirectory;
            dir = dir.Replace('/', '\\');
            dir = currentWorkingDirectory + dir;
            dir = dir.Replace("\\\\", "\\");
            var tmp = dir.Split('\\').ToList();
            for (var i = 0; i < tmp.Count; )
            {
                if (tmp[i] == "..")
                {
                    tmp.RemoveAt(i);
                    if (i > 0)
                    {
                        tmp.RemoveAt(i - 1);
                    }
                }
                else
                {
                    i++;
                }
            }
            return string.Join("\\", tmp.ToArray());
        }
        public string GetFullPath(string dir)
        {
            dir = GetPath(dir);
            dir = root + dir;
            return dir.Replace("\\\\", "\\");
        }
        public bool ChangeWorkingDirectory(string dir)
        {
            dir = GetPath(dir);
            var fullPath = root + dir;
            fullPath = fullPath.Replace("\\\\", "\\");
            if (Directory.Exists(fullPath))
            {
                currentWorkingDirectory = dir;
                return true;
            }
            else
                return false;
        }

        public string CurrentPath
        {
            get { return Path.Combine(root, currentWorkingDirectory); }
        }
    }
}
