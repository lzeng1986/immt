using LazyBones.Communication.Messages;

namespace LazyBones.Communication.Apps.Ftp
{
    public class FtpMessage : LBTextMessage
    {
        public override string Text
        {
            get { return Cmd + ' ' + Value; }
            set
            {
                var ind = value.IndexOf(' ');
                if (ind == -1)
                {
                    Cmd = value.Trim();
                    Value = string.Empty;
                }
                else
                {
                    Cmd = value.Substring(0, ind).Trim();
                    Value = value.Substring(ind + 1).Trim();
                }
            }
        }
        public string Cmd { get; set; }
        public string Value { get; set; }
        public override string ToString()
        {
            return Text;
        }
    }
}
