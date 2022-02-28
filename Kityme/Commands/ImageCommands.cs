using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Kityme.Commands {
    public class ImageCommands : BaseCommandModule
    {
        [Command("border")]
        public async Task Border (CommandContext ctx, byte r = 0, byte g = 0, byte b = 0, [RemainingText] DiscordMember member = null) {
            member ??= ctx.Member;
            var color = Color.FromRgb(r, g, b);

            Stream avatarStream = null;
            using(var client = new HttpClient()) {
                avatarStream = await client.GetStreamAsync(member.GetAvatarUrl(ImageFormat.Png, 1024));
            }

            using(var img = await Image<Rgba32>.LoadAsync(avatarStream)) {
                img.Mutate(x => x.Resize(1024, 1024));
                var withBorder = new Image<Rgba32>(img.Width, img.Height);
                img.Mutate(x => x.ConvertToAvatar(new Size(920), 920 /2));
                withBorder.Mutate(x =>
                    x.BackgroundColor(color)
                    .DrawImage(img, new Point((withBorder.Width - img.Width) /2, (withBorder.Height - img.Height) /2), 1)
                );
                

                var ms = new MemoryStream();
                withBorder.Save(ms, new PngEncoder());
                ms.Position = 0;

                DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder()
                    .WithFile("kityme-border.png", ms)
                    .WithContent("ta ai o avatar com bordinha");

                await ctx.RespondAsync(messageBuilder);

            }

        }

        [Command("bordergradient")]
        public async Task BorderGradient (CommandContext ctx, [RemainingText] DiscordMember member = null) {
            member ??= ctx.Member;
            var colors = new ColorStop[]
            {
                new ColorStop(0f, Color.FromRgb(215, 2, 112)),
                new ColorStop(0.375f, Color.FromRgb(115, 79, 150)),
                new ColorStop(0.85f, Color.FromRgb(0, 56, 168))
            };

            Stream avatarStream = null;
            using(var client = new HttpClient()) {
                avatarStream = await client.GetStreamAsync(member.GetAvatarUrl(ImageFormat.Png, 1024));
            }

            using(var img = await Image<Rgba32>.LoadAsync(avatarStream)) {
                img.Mutate(x => x.Resize(1024, 1024));
                var withBorder = new Image<Rgba32>(img.Width, img.Height);
                img.Mutate(x => x.ConvertToAvatar(new Size(920), 920 /2));
                withBorder.Mutate(x => {
                    var options = new DrawingOptions();
                    var point1 = new PointF(512, 0);
                    var point2 = new PointF(512, 1024);
                    var a = new LinearGradientBrush(point1, point2, GradientRepetitionMode.None, colors);
                    x.Fill(options, a)
                    .DrawImage(img, new Point((withBorder.Width - img.Width) /2, (withBorder.Height - img.Height) /2), 1);
                });
                

                var ms = new MemoryStream();
                withBorder.Save(ms, new PngEncoder());
                ms.Position = 0;

                DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder()
                    .WithFile("kityme-border.png", ms)
                    .WithContent("ta ai o avatar com bordinha");

                await ctx.RespondAsync(messageBuilder);

            }
        }
    }


    public static class Avatar {
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
                AlphaCompositionMode = PixelAlphaCompositionMode.DestOut // enforces that any part of this shape that has color is punched out of the background
            });
            
            // mutating in here as we already have a cloned original
            // use any color (not Transparent), so the corners will be clipped
            foreach (var c in corners)
            {
                ctx = ctx.Fill(Color.Coral, c);
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