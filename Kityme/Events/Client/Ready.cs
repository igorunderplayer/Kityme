using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.Entities;
using System.Timers;
using System.Net.Http;
using System.IO;

namespace Kityme.Events.Client
{
    public class Ready
    {
        private readonly DiscordClient _client;
        public Ready (DiscordClient client)
        {
            this._client = client;
        }

        public Task Client_Ready (DiscordClient client, ReadyEventArgs e)
        {
            DiscordActivity[] activities =
            {
                new DiscordActivity { Name = "pessoas falarem merda", ActivityType = ActivityType.ListeningTo },
                new DiscordActivity { Name = "gemidos", ActivityType = ActivityType.ListeningTo },
                new DiscordActivity { Name = "fofo", ActivityType = ActivityType.ListeningTo },
                new DiscordActivity { Name = "hentai", ActivityType = ActivityType.Watching },
                new DiscordActivity { Name = $"para {client.Guilds.Count} servidores", ActivityType = ActivityType.Streaming, StreamUrl = "https://twitch.tv/..." }
            };

            Console.WriteLine("Logged as " + client.CurrentUser.Username);
            client.UpdateStatusAsync(activities[new Random().Next(0, activities.Length)]);
            StatusChanger(client);
            AvatarChanger(client);

            return Task.CompletedTask;
        }

        public void AvatarChanger(DiscordClient client)
        {
            Timer timer = new Timer(1000 * 60 * 15); // 15 minutos
            timer.AutoReset = true;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();

            async void Timer_Elapsed(object sender, ElapsedEventArgs e)
            {
                string[] avatars =
                    {
                        "https://cdn.discordapp.com/attachments/857013415704920065/857013764023648276/01f4f2b71dff92e4cfe3fb996e3d618b.jpg",
                        "https://cdn.discordapp.com/attachments/857013415704920065/857013623296753704/10b7f7e2787310bf6442942b734f6411.jpg",
                        "https://cdn.discordapp.com/attachments/857013415704920065/857013622961602630/61e42824e50a8edc6aacc483cdf9c230.jpg",
                        "https://cdn.discordapp.com/attachments/857013415704920065/857013622496428042/a3d0b61c733bacf75385155b247e56c6.jpg",
                        "https://cdn.discordapp.com/attachments/857013415704920065/857013622299688970/0bc16089e04fccb8d2574270ce5b701c.jpg",
                        "https://cdn.discordapp.com/attachments/857013415704920065/857013622038724628/0d4e710895a7f9780e91ef05fb53f9ba.jpg",
                        "https://cdn.discordapp.com/attachments/857013415704920065/857013621808431114/4fef89464b332df5da025330a8b56b67.jpg",
                        "https://cdn.discordapp.com/attachments/857013415704920065/857013621581414450/be867c80eabd2ca0907512961f6f5626.jpg",
                        "https://cdn.discordapp.com/attachments/857013415704920065/857014167528669224/e351abad88c1e472bd7e72f95674db70.jpg",
                        "https://cdn.discordapp.com/attachments/857013415704920065/857014167810342912/70b21b2307a00ff7564cbbdb5b05dc82.jpg",
                        "https://cdn.discordapp.com/attachments/857013415704920065/869271027295858688/a454196903c4fca343d93e1fc1363f76.jpg",
                        "https://cdn.discordapp.com/attachments/857013415704920065/869271027522359336/21e13da2c5eef4cb0a02c9335fbbae22.jpg",
                        "https://cdn.discordapp.com/attachments/857013415704920065/869271027778224168/087c3212547686501e22bf47242d0cd4.jpg",
                        "https://cdn.discordapp.com/attachments/857013415704920065/869271028298293348/77f99c946c1d9e2963860249108e622b.jpg",
                        "https://cdn.discordapp.com/attachments/857013415704920065/869271028495429712/3147c2683c92b93b13ac5f32244f62a8.jpg",
                        "https://cdn.discordapp.com/attachments/857013415704920065/869271028709351424/453d2e58c320e566a745a00dc4405db5.jpg",
                        "https://cdn.discordapp.com/attachments/857013415704920065/869271028956823612/ce29e2a011863b8c4cb6f2ef4a588d6c.jpg",
                        "https://cdn.discordapp.com/attachments/857013415704920065/869271029363642408/b91ef5c1faad701171736205adba500d.jpg",
                        "https://cdn.discordapp.com/attachments/857013415704920065/869271029732765716/59158b32ff12f2aff4186a3fa42a7b16.jpg",
                        "https://cdn.discordapp.com/attachments/857013415704920065/869271092089483304/a01cb1654df8fcf5b7301896ba03816a.jpg",
                        "https://cdn.discordapp.com/attachments/857013415704920065/869271092685049896/923d5f5e54868c0b38ba7e8843947285.jpg",
                        "https://cdn.discordapp.com/attachments/857013415704920065/869271092936724500/191bdd5f86e72bd3903e5997135cd6a9.jpg",
                        "https://cdn.discordapp.com/attachments/857013415704920065/869271093167403048/f72c3fa0e701afa88e673581754bb4a5.jpg",
                        "https://cdn.discordapp.com/attachments/857013415704920065/869271093347778610/36426769cc11161a14704b878e5ae273.jpg",
                        "https://cdn.discordapp.com/attachments/857013415704920065/869271093532311602/eba455b5d3cf745617ca8f6d313df5cb.jpg",
                        "https://cdn.discordapp.com/attachments/857013415704920065/869271094119501915/fcca3543e9e05538828e3ae55f1accd1.jpg",
                        "https://cdn.discordapp.com/attachments/857013415704920065/869271094266323034/6a397bb735689ad5962c68db00d9f68f.jpg",
                        "https://cdn.discordapp.com/attachments/857013415704920065/869271154299371570/13f3e52556414c16428c70f4e5bdcf83.jpg",
                        "https://cdn.discordapp.com/attachments/857013415704920065/869271154743975966/0ec7f9a059c8b0d45349e2589ae2f03b.jpg",
                        "https://cdn.discordapp.com/attachments/857013415704920065/869271155436060772/fc65165f84edc87a4e2362303027eb19.jpg",
                        "https://cdn.discordapp.com/attachments/857013415704920065/869271155838709810/a2f86707aae19c5ae338dbdad6366f8f.jpg",
                        "https://cdn.discordapp.com/attachments/857013415704920065/869271156253921380/e19edfcee3581f3cb8bc4958a636e33d.jpg",
                        "https://cdn.discordapp.com/attachments/857013415704920065/869271156551745586/5d3229fda31f9d8943f20aa2715e0210.jpg",
                        "https://cdn.discordapp.com/attachments/857013415704920065/869271156933406801/b9e4c8080a340a0bd37520136d7ce5a9.jpg"
                    };
                using (HttpClient httpClient = new HttpClient())
                {
                    string avatar = avatars[new Random().Next(0, avatars.Length)];
                    var res = await httpClient.GetAsync(avatar);
                    Stream str = await res.Content.ReadAsStreamAsync();
                    try
                    {
                        await client.UpdateCurrentUserAsync(null, str);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Tentei troca de avatar mas deu erro, provavelmente to trocando rapido d+");
                    }
                }
            }

        }

        public void StatusChanger(DiscordClient client)
        {
            Timer timer = new Timer(1000 * 20); // 20 segundos
            timer.AutoReset = true;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();

            async void Timer_Elapsed(object sender, ElapsedEventArgs e)
            {
                DiscordActivity[] activities =
            {
                new DiscordActivity { Name = "pessoas falarem merda", ActivityType = ActivityType.ListeningTo },
                new DiscordActivity { Name = $"para {client.Guilds.Count} servidores", ActivityType = ActivityType.Streaming, StreamUrl = "https://twitch.tv/..." },
                new DiscordActivity { Name = "sou fofo", ActivityType = ActivityType.ListeningTo },
                new DiscordActivity { Name = "reuprytmin", ActivityType = ActivityType.Playing },
                new DiscordActivity { Name = "vc me adicionar no seu servidor", ActivityType = ActivityType.Watching }
            };

                await client.UpdateStatusAsync(activities[new Random().Next(0, activities.Length)]);
            }
        }
    }
}
