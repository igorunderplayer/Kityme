using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Gif;


namespace Kityme.Commands
{
    public class ImageCommands : BaseCommandModule
    {
        [Command("pixelate")]
        public async Task PixelateCommand(CommandContext ctx, int pixelSize = -1, [RemainingText] DiscordMember member = null)
        {
            member ??= ctx.Member;

            using (HttpClient client = new HttpClient())
            using (Stream avatarStream = await client.GetStreamAsync(member.GetAvatarUrl(ImageFormat.Png, 1024)))
            using (Image avatar = await Image.LoadAsync(avatarStream))
            {
                if (pixelSize <= 0) pixelSize = (avatar.Height + avatar.Width) / 128;
                avatar.Mutate(x => x.Pixelate(pixelSize));
                client.Dispose();
                await avatarStream.DisposeAsync();

                using (MemoryStream ms = new MemoryStream())
                {
                    await avatar.SaveAsync(ms, new PngEncoder());
                    ms.Position = 0;

                    DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder()
                        .WithFile("pixelated.png", ms)
                        .WithContent("PIXELS");

                    await ctx.RespondAsync(messageBuilder);
                }
            }
        }

        [Command("border")]
        public async Task Border(CommandContext ctx, DiscordColor color, [RemainingText] DiscordMember member = null)
        {
            member ??= ctx.Member;

            using (HttpClient client = new HttpClient())
            using (Stream avatarStream = await client.GetStreamAsync(member.GetAvatarUrl(ImageFormat.Png, 1024)))
            using (Image original = Image.Load(avatarStream, out var format))
            using (Image img = original.CloneAs<Rgba32>())
            {
                client.Dispose();
                await avatarStream.DisposeAsync();

                Color c = Color.FromRgb(color.R, color.G, color.B);
                img.Mutate(x => x.Resize(1024, 1024).BackgroundColor(c));
                using (Image withBorder = new Image<Rgba32>(img.Width, img.Height))
                {
                    img.Mutate(x => x.ConvertToAvatar(new Size(920), 920 / 2));
                    withBorder.Mutate(x =>
                    {
                        x.BackgroundColor(c)
                        .DrawImage(img, new Point((withBorder.Width - img.Width) / 2, (withBorder.Height - img.Height) / 2), 1);
                    });

                    using (MemoryStream ms = new MemoryStream())
                    {
                        await withBorder.SaveAsync(ms, format.Name.ToLower() == "gif" ? new GifEncoder() : new PngEncoder());
                        ms.Position = 0;

                        string filename = format.Name.ToLower() == "gif" ? "kityme-border.gif" : "kityme-border.png";

                        DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder()
                            .WithFile(filename, ms)
                            .WithContent("ta ai o avatar com bordinha");

                        await ctx.RespondAsync(messageBuilder);
                    }
                }
            }
        }

        [Command("bordergradient")]
        public async Task BorderGradient(CommandContext ctx, params DiscordColor[] rawColors)
        {
            DiscordMember member = ctx.Member;

            if (rawColors.Length < 2)
            {
                await ctx.RespondAsync("vc tem q colocar pelo menos 2 cores. ex: `43,45,0 12,76,255`");
                return;
            }

            ColorStop[] colors = new ColorStop[rawColors.Length];

            for (int i = 0; i < rawColors.Length; i++)
            {
                DiscordColor color = rawColors[i];
                float point = ((1f / rawColors.Length) * (float)(i + .5f));
                colors[i] = new ColorStop(point, Color.FromRgb(color.R, color.G, color.B));
            }

            HttpClient client = new HttpClient();
            Stream avatarStream = await client.GetStreamAsync(member.GetAvatarUrl(ImageFormat.Png, 1024));
            client.Dispose();
            Image result = Avatar.GenerateBorderGradientAvatar(avatarStream, colors);
            avatarStream.Close();
            MemoryStream ms = new MemoryStream();

            await result.SaveAsync(ms, new PngEncoder());
            ms.Position = 0;

            DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder()
                    .WithFile("kityme-border.png", ms)
                    .WithContent("ta ai o avatar com bordinha (COMANDO EM TESTES!!!)");

            await ctx.RespondAsync(messageBuilder);

            result.Dispose();
            ms.Close();
        }
    }


    public static class Avatar
    {
        public static Image GenerateBorderGradientAvatar(Stream avatar, ColorStop[] colors)
        {
            using (var original = Image.Load(avatar, out var format))
            using (var img = original.CloneAs<Rgba32>())
            {
                img.Mutate(x => x.Resize(1024, 1024));
                using (var withBorder = new Image<Rgba32>(img.Width, img.Height))
                {
                    img.Mutate(x => x.ConvertToAvatar(new Size(920), 920 / 2));
                    withBorder.Mutate(x =>
                    {
                        var options = new DrawingOptions();
                        var point1 = new PointF(img.Width / 2, 0);
                        var point2 = new PointF(img.Width / 2, img.Height);
                        var a = new LinearGradientBrush(point1, point2, GradientRepetitionMode.None, colors);
                        x.Fill(options, a)
                        .DrawImage(img, new Point((withBorder.Width - img.Width) / 2, (withBorder.Height - img.Height) / 2), 1);
                    });

                    return withBorder;
                }
            }
        }

        // https://github.com/SixLabors/Samples/blob/main/ImageSharp/AvatarWithRoundedCorner/Program.cs
        public static IImageProcessingContext ConvertToAvatar(this IImageProcessingContext processingContext, Size size, float cornerRadius)
        {
            return processingContext.Resize(new ResizeOptions
            {
                Size = size,
                Mode = ResizeMode.Crop
            }).ApplyRoundedCorners(cornerRadius);
        }


        // This method can be seen as an inline implementation of an `IImageProcessor`:
        // (The combination of `IImageOperations.Apply()` + this could be replaced with an `IImageProcessor`)
        private static IImageProcessingContext ApplyRoundedCorners(this IImageProcessingContext ctx, float cornerRadius)
        {
            Size size = ctx.GetCurrentSize();
            IPathCollection corners = BuildCorners(size.Width, size.Height, cornerRadius);

            ctx.SetGraphicsOptions(new GraphicsOptions()
            {
                Antialias = true,
                AlphaCompositionMode = PixelAlphaCompositionMode.DestOut
            });

            // mutating in here as we already have a cloned original
            // use any color (not Transparent), so the corners will be clipped
            foreach (var c in corners)
            {
                ctx = ctx.Fill(Color.Red, c);
            }
            return ctx;
        }

        private static IPathCollection BuildCorners(int imageWidth, int imageHeight, float cornerRadius)
        {
            // first create a square
            var rect = new RectangularPolygon(-0.5f, -0.5f, cornerRadius, cornerRadius);

            // then cut out of the square a circle so we are left with a corner
            IPath cornerTopLeft = rect.Clip(new EllipsePolygon(cornerRadius - 0.5f, cornerRadius - 0.5f, cornerRadius));

            // corner is now a corner shape positions top left
            //lets make 3 more positioned correctly, we can do that by translating the original around the center of the image

            float rightPos = imageWidth - cornerTopLeft.Bounds.Width + 1;
            float bottomPos = imageHeight - cornerTopLeft.Bounds.Height + 1;

            // move it across the width of the image - the width of the shape
            IPath cornerTopRight = cornerTopLeft.RotateDegree(90).Translate(rightPos, 0);
            IPath cornerBottomLeft = cornerTopLeft.RotateDegree(-90).Translate(0, bottomPos);
            IPath cornerBottomRight = cornerTopLeft.RotateDegree(180).Translate(rightPos, bottomPos);

            return new PathCollection(cornerTopLeft, cornerBottomLeft, cornerTopRight, cornerBottomRight);
        }
    }

}