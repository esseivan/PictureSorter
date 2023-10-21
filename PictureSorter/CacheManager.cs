using ESNLib.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PictureSorter
{
    public class CacheManager
    {
        /// <summary>
        /// Number of cached images
        /// </summary>
        public static int CACHE_MAX_SIZE => AppSettingsManager.Instance.CachedImagesCount;

        /// <summary>
        /// Queue (fifo) of cached images
        /// </summary>
        private static readonly List<ImageInfo> imagesCached = new List<ImageInfo>(CACHE_MAX_SIZE);

        /// <summary>
        /// Add the list to the cache, removing all the old ones
        /// </summary>
        public void Add(IEnumerable<ImageInfo> imageInfos)
        {
            if (imageInfos.Count() > CACHE_MAX_SIZE)
                throw new ArgumentOutOfRangeException(
                    nameof(ImageInfo),
                    $"The size cannot be greater than {CACHE_MAX_SIZE}"
                );

            // remove already cached items. Then they won't call Decache
            foreach (ImageInfo item in imageInfos)
            {
                if (imagesCached.Contains(item))
                    imagesCached.Remove(item);
            }

            // Dechache the required amount of images.
            int amountToDecache = Math.Max(
                0,
                imageInfos.Count() + imagesCached.Count - CACHE_MAX_SIZE
            );
            Logger.Instance.Write(
                $"[CacheManager] {amountToDecache} to decache... ({imageInfos.Count() + imagesCached.Count}/{CACHE_MAX_SIZE})"
            );

            // Remove the image cached in slot 0, then delete that slot, shifting the whole list.
            for (int i = 0; i < amountToDecache; i++)
            {
                imagesCached[0].Decache();
                imagesCached.RemoveAt(0);
            }
            Logger.Instance.Write($"[CacheManager] Decache complete. Preparing to cache...");

            GC.Collect();

            // Fill the cache. Most recent images will be at the end of the list
            imagesCached.AddRange(imageInfos);

            // Call cache for all. If already, nothing will happen
            imagesCached.ForEach(
                (item) =>
                {
                    item.Cache();
                }
            );
        }

        /// <summary>
        /// Decache and clear all
        /// </summary>
        public void Clear()
        {
            imagesCached.ForEach(
                (item) =>
                {
                    try
                    {
                        item.Decache();
                    }
                    catch (Exception) { }
                }
            );
            imagesCached.Clear();
        }
    }
}
