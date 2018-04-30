﻿using System;
using Sitecore.Data.Items;
using Sitecore.Resources.Media;
using Sitecore.Diagnostics;
using Sitecore.IO;
using Sitecore.Web;

namespace Sitecore.Support.Resources.Media
{
#pragma warning disable CS0612 // Type or member is obsolete
  public class MediaProvider : Sitecore.Resources.Media.MediaProvider
  {
    /// <summary>
    /// Gets a media URL.
    /// </summary>
    /// <param name="item">The media item.</param>
    /// <param name="options">The query string.</param>
    /// <returns>The media URL.</returns>
    [NotNull]
    public override string GetMediaUrl(MediaItem item, MediaUrlOptions options)
    {
      Assert.ArgumentNotNull(item, "item");
      Assert.ArgumentNotNull(options, "options");

      Assert.IsTrue(this.Config.MediaPrefixes[0].Length > 0, "media prefixes are not configured properly.");
      string prefix = this.MediaLinkPrefix;

      if (options.AbsolutePath)
      {
        prefix = options.VirtualFolder + prefix;
      }
      else if (prefix.StartsWith("/", StringComparison.InvariantCulture))
      {
        prefix = StringUtil.Mid(prefix, 1);
      }

      prefix = MainUtil.EncodePath(prefix, '/');

      if (options.AlwaysIncludeServerUrl)
      {
        prefix = FileUtil.MakePath(string.IsNullOrEmpty(options.MediaLinkServerUrl) ? WebUtil.GetServerUrl() : options.MediaLinkServerUrl, prefix, '/');
      }

      string extension = StringUtil.GetString(options.RequestExtension, item.Extension, Constants.AshxExtension);

      extension = StringUtil.EnsurePrefix('.', extension);

      string parameters = options.ToString();

      if (options.AlwaysAppendRevision)
      {        
        if (!string.IsNullOrEmpty(item.InnerItem.Statistics.Revision)) // Fix bug: 219261
        {
          // TODO: take into account that item may not have version in given language
          var rev = Guid.Parse(item.InnerItem.Statistics.Revision)
            .ToString("N"); // lowercase digits without hypens, braces and parentheses

          parameters = string.IsNullOrEmpty(parameters) ? "rev=" + rev : parameters + "&rev=" + rev;
        }
      }

      if (parameters.Length > 0)
      {
        extension += "?" + parameters;
      }

      string mediaRoot = Constants.MediaLibraryPath + "/";
      string itemPath = item.InnerItem.Paths.Path;

      string path;

      // Check whether an item is under media library, 
      // otherwise it would be hard to define whether it's relative path to an item or absolute
      if (options.UseItemPath
        && itemPath.StartsWith(mediaRoot, StringComparison.OrdinalIgnoreCase))
      {
        path = StringUtil.Mid(itemPath, mediaRoot.Length);
      }
      else
      {
        path = item.ID.ToShortID().ToString();
      }

      path = MainUtil.EncodePath(path, '/');
      path = prefix + path + (options.IncludeExtension ? extension : string.Empty);
      return options.LowercaseUrls ? path.ToLowerInvariant() : path;
    }
  }
#pragma warning restore CS0612 // Type or member is obsolete
}
