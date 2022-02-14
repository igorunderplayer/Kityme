using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;

using Kityme.Managers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Kityme.Minigames
{
  public class SlidePuzzle
  {
    public DiscordClient Client { get; private set; }
    public DiscordMember Player { get; private set; }
    public int Size { get; private set; }
    public Image OriginalImage { get; private set; }
    public DiscordChannel Channel { get; private set; }
    public DiscordMessage LastMessage { get; private set; }

    public Tile[,] Tiles { get; private set; }
    public string[,] OriginalTilesId { get; private set; }

    public SlidePuzzle(DiscordClient client, DiscordMember playerMember, DiscordChannel channel, int size = 2)
    {
      this.Client = client;
      this.Player = playerMember;
      this.Channel = channel;
      this.Size = size;

      this.Tiles = new Tile[size, size];
      this.OriginalTilesId = new string[size,size];
    }

    public async Task Start()
    {
      ulong[] discordKeys = this.Channel.Guild.Members.Keys.ToArray();
      ulong key = (ulong)new Random().Next(0, discordKeys.Length);
      ulong memberID = discordKeys[key];
      DiscordMember randomMember = await this.Channel.Guild.GetMemberAsync(memberID);

      var client = new HttpClient();
      var avatarStream = await client.GetStreamAsync(randomMember.GetAvatarUrl(ImageFormat.Png, 1024));
      this.OriginalImage = await Image.LoadAsync(avatarStream);
      this.OriginalImage.Mutate(x => x.Resize(1024, 1024));


      for (int i = 0; i < this.Tiles.GetLength(0); i++)
      {
        for (int j = 0; j < this.Tiles.GetLength(1); j++)
        {
          var point = new Point(i * (OriginalImage.Height / Size), j * (OriginalImage.Width / Size));
          var size = new Size(OriginalImage.Width / Size, OriginalImage.Height / Size);
          var rectangle = new Rectangle(point, size);
          Image img = OriginalImage.Clone(x => x
               .Crop(rectangle)
          );
          this.Tiles[i, j] = new Tile(img, $"{i},{j}");
          this.OriginalTilesId[i, j] = $"{i},{j}";
        }
      }

      Tiles[Tiles.GetLength(0) - 1, Tiles.GetLength(1) - 1] = null;
      Random rand = new Random();
      Shuffle(rand, this.Tiles);
    }

    public bool Solved()
    {
      for (int i = 0; i < Tiles.GetLength(0); i++)
      {
        for (int j = 0; j < Tiles.GetLength(1); j++)
        {
          if (Tiles[i, j]?.id != OriginalTilesId[i, j]) return false;
        }
      }

      return true;
    }

    public static SlidePuzzle Create(DiscordClient client, DiscordMember member, DiscordChannel channel, ushort size)
    {
        if (size > 5) return null;
        if (Managers.Minigames.SlidePuzzle.ContainsKey(member.Id)) return null;

        SlidePuzzle game = new SlidePuzzle(client, member, channel, size);
        Managers.Minigames.SlidePuzzle.Add(member.Id, game);
        return game;
    }

    public async Task<DiscordMessage> UpdateMessage()
    {
        Image image = new Image<Rgba32>(1024, 1024);
        MemoryStream ms = new MemoryStream();
        for (int i = 0; i < Tiles.GetLength(0); i++)
        {
        for (int j = 0; j < Tiles.GetLength(1); j++)
        {
            var tile = this.Tiles[i, j];
            Point point = new Point(i * (OriginalImage.Height / Size), j * (OriginalImage.Width / Size));
            if (tile?.image != null)
            image.Mutate(x => x.DrawImage(tile.image, point, 1));
        }
        }

        image.Save(ms, new PngEncoder());
        ms.Position = 0;

        DiscordMessageBuilder builder = new DiscordMessageBuilder()
            .WithFile("image.png", ms);

        for (int i = 0; i < Tiles.GetLength(0); i++)
        {
            DiscordComponent[] components = new DiscordComponent[Size];
            for (int c = 0; c < Tiles.GetLength(1); c++)
            {
                FindBlank(out int x, out int y);
                components[c] = new DiscordButtonComponent(ButtonStyle.Secondary, $"slidePuzzle_{c},{i}", $"{c}-{i}");
            }
            builder.AddComponents(components);
        }

        if (LastMessage != null)
        {
            await LastMessage.DeleteAsync();
            // LastMessage = await LastMessage.ModifyAsync(builder);
            // return LastMessage;
        }

        LastMessage = await this.Channel.SendMessageAsync(builder);
        return LastMessage;
    }

    public async Task HandleInteraction(DiscordInteraction interaction)
    {
      if (interaction.User.Id != this.Player.Id) return;
      var x = int.Parse(interaction.Data.CustomId.Replace("slidePuzzle_", "").Split(',')[0]);
      var y = int.Parse(interaction.Data.CustomId.Replace("slidePuzzle_", "").Split(',')[1]);

      Move(x, y);
      await interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
      await UpdateMessage();
      if (Solved())
      {
        await interaction.Channel.SendMessageAsync($"{interaction.User.Mention} parabens! vc completo o puzzle");
        Managers.Minigames.SlidePuzzle.Remove(interaction.User.Id);
      }
    }

    public void FindBlank(out int x, out int y)
    {
      x = 0;
      y = 0;
      for (int i = 0; i < Tiles.GetLength(0); i++)
      {
        for (int j = 0; j < Tiles.GetLength(1); j++)
        {
          if (Tiles[i, j] == null)
          {
            x = i;
            y = j;
          }
        }
      }
    }

    public void Move(int x, int y)
    {
      FindBlank(out int blankX, out int blankY);
      if (IsNeighbor(x, y, blankX, blankY))
      {
        Swap(x, y, blankX, blankY, Tiles);
      }
    }

    public void Swap(int x1, int y1, int x2, int y2, Tile[,] arr)
    {
      Tile temp = arr[x1, y1];
      arr[x1, y1] = arr[x2, y2];
      arr[x2, y2] = temp;
    }

    public bool IsNeighbor(int x1, int y1, int x2, int y2)
    {
      if (Math.Abs(x1 - x2) == 1 || Math.Abs(y1 - y2) == 1)
      {
        return true;
      }
      return false;
    }

    private void Shuffle(Random random, Tile[,] array)
    {
      int lengthRow = array.GetLength(1);

      for (int i = array.Length - 1; i > 0; i--)
      {
        int i0 = i / lengthRow;
        int i1 = i % lengthRow;

        int j = random.Next(i + 1);
        int j0 = j / lengthRow;
        int j1 = j % lengthRow;

        Tile temp = array[i0, i1];
        array[i0, i1] = array[j0, j1];
        array[j0, j1] = temp;
      }
    }

  }

  public class Tile
  {
    public Image image = null;
    public string id;
    public Tile(Image img, string id)
    {
      this.image = img;
      this.id = id;
    }
  }
}