namespace Kityme
{
    public class BotConfig
    {
        public string Token { get; private set; }
        public  string LavalinkHost { get; private set; }
        public  string SLavalinkHost { get; private set; }
        public  string LavalinkPassword { get; private set; }
        public  string MongoUrl { get; private set; }

        public BotConfig(string token, string lavaHost, string sLavaHost, string lavaPass, string mongoUrl)
        {
            this.Token = token;
            this.LavalinkHost = lavaHost;
            this.SLavalinkHost = sLavaHost;
            this.LavalinkPassword = lavaPass;
            this.MongoUrl = mongoUrl;
        }
    }
}