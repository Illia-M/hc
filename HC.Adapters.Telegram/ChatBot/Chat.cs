namespace HC.Adapters.Telegram.ChatBot
{
    public class Chat
    {
        public static Chat Create(string id)
        {
            return new Chat(id);
        }
        public static Chat Create(long id)
        {
            return new Chat(id.ToString());
        }

        private Chat(string id)
        {
            Id = id;
            Lang = "en";
        }

        public string Id {
            get;
        }

        public string Lang {
            get;
        }
    }
}