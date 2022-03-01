using System;
using System.IO;
using System.Net.Http;
using System.Collections.Generic;
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
using System.Text;
using MongoDB.Bson.Serialization.Attributes;

namespace Kityme.Commands {
    public class ImageCommands : BaseCommandModule
    {
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
        public async Task BorderGradient (CommandContext ctx, params DiscordColor[] rawColors) {
            var member = ctx.Member;

            if(rawColors.Length < 2) {
                await ctx.RespondAsync("vc tem q colocar pelo menos 2 cores. ex: `43,45,0 12,76,255`");
                return;
            }

            ColorStop[] colors = new ColorStop[rawColors.Length];
            
            for (int i = 0;i < rawColors.Length;i++) {
                DiscordColor color = rawColors[i];
                float point = ((1f / rawColors.Length) * (float)(i +.5f));
                colors[i] = new ColorStop(point, Color.FromRgb(color.R, color.G, color.B));
            }

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
                    .WithContent("ta ai o avatar com bordinha (COMANDO EM TESTES!!!)");

                await ctx.RespondAsync(messageBuilder);

            }
        }

        [Command("bordergradient-preset")]
        public async Task BorderBackgroundPreset (CommandContext ctx, string action, string name = null, params DiscordColor[] rawColors) {
            var presets = await Managers.DBManager.GetAllPresets();
            switch (action) {
                case "create":
                    if(string.IsNullOrWhiteSpace(name)) return;
                    if(presets.Exists(f => f.name == name)) {
                        await ctx.RespondAsync("ja existe um preset com esse nome ae!");
                        return;
                    } else {
                        PresetColor[] colors = new PresetColor[rawColors.Length];
            
                        for (int i = 0;i < rawColors.Length;i++) {
                            DiscordColor color = rawColors[i];
                            float point = ((1f / rawColors.Length) * (float)(i +.5f));
                            colors[i] = new PresetColor(color.R, color.G, color.B);
                        }

                        PresetBorderGradient newPreset = new PresetBorderGradient(colors, name);

                        await Managers.DBManager.BorderGradientPresetCollection.InsertOneAsync(newPreset);

                        await ctx.RespondAsync($"preset com nome {name} criado!");
                    }
                break;

                case "load":
                    if(string.IsNullOrWhiteSpace(name)) return;
                    if(!presets.Exists(f => f.name == name)) return;
                    PresetBorderGradient presetToLoad = presets.Find(x => x.name == name);

                    StringBuilder stringBuilder = new();
                    stringBuilder
                        .Append("use o comando abaixo:")
                        .AppendLine()
                        .Append("`<prefix>bordergradient` ");

                    foreach(var color in presetToLoad.colors) {
                        stringBuilder.Append($"{color.r},{color.g},{color.b} ");
                    }

                    await ctx.RespondAsync(stringBuilder.ToString());
                break;

                default:
                    return;
            }
        }

        [Command("bordergradient-preset")]
        public async Task BorderBackgroundPreset(CommandContext ctx, string action = "list") {
            var presets = await Managers.DBManager.GetAllPresets();
            if (action == "list") {
                string msg = "use `<prefix> bordergradient-preset load [nome]` \n\n";
                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                    .WithTitle("lista de presets para o comando bordergradient");

                foreach(var dbPreset in presets) {
                    msg += ($"`{dbPreset.name}` \n");
                }

                embedBuilder.WithDescription(msg);

                await ctx.RespondAsync(embedBuilder);
            }
        }

        [Command("teste")]
        public async Task Teste (CommandContext ctx, DiscordColor color) {
            await ctx.RespondAsync($"color: val {color.Value} | r: {color.R} g: {color.G} b: {color.B}");
        }
    }

    [BsonIgnoreExtraElements]
    public class PresetBorderGradient {
        public string name;
        public PresetColor[] colors;

        public PresetBorderGradient (PresetColor[] colors, string name) {
            this.name = name;
            this.colors = colors;       
        }
    }

    public class PresetColor {
        public byte r = 0;
        public byte g = 0;
        public byte b = 0;
        public PresetColor (byte r, byte g, byte b) {
            this.r = r;
            this.g = g;
            this.b = b;
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