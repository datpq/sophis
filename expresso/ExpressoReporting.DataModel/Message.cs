namespace ExpressoReporting.DataModel
{
    public enum MessageCode
    {
        Unknown = 12976,
        UserLoginFailed,
        MissingToken,
        UserNotFound,
        UserTokenError
    }

    public class Message
    {
        public static readonly Message MsgMissingToken = new Message { Code = MessageCode.MissingToken, Msg = "Missing Token" };
        public static readonly Message MsgUnknown = new Message { Code = MessageCode.Unknown, Msg = "Unknown error" };
        public static readonly Message MsgUserNotFound = new Message { Code = MessageCode.UserNotFound, Msg = "User not found." };
        public static readonly Message MsgWrongToken = new Message { Code = MessageCode.UserTokenError, Msg = "Wrong Token" };

        public MessageCode Code { get; set; }
        public string Msg { get; set; }

        public override string ToString()
        {
            return $"{Code}: {Msg}";
        }
    }
}
