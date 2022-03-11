using FuseSharp;
using System.Collections.Generic;
using System.Diagnostics;

namespace The_Emojinator
{
	public class Emoji : IFuseable
	{
        public string? Name { get; set; }

        public string Url {
            get
            {
                return $"https://emoji-server.azurewebsites.net/emoji/{Name}";
            }
            private set { }
        }

		public IEnumerable<FuseProperty> Properties => new[]
		{
            new FuseProperty(Name ?? "", 1.0),
            new FuseProperty(Url, 0.0)
		};
	}
}
