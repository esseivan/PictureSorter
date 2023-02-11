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
        public const int CACHE_SIZE = 3;

        /// <summary>
        /// Queue (fifo) of cached images
        /// </summary>
        private static readonly List<ImageInfo> imagesCached = new List<ImageInfo>(CACHE_SIZE);

        /// <summary>
        /// Add the list to the cache, removing all the old ones
        /// </summary>
        public void Add(IEnumerable<ImageInfo> imageInfos)
        {
            if (imageInfos.Count() > CACHE_SIZE)
                throw new ArgumentOutOfRangeException(
                    nameof(ImageInfo),
                    $"The size cannot be greater than {CACHE_SIZE}"
                );

            // remove already cached items. Then they won't call Decache
            foreach (ImageInfo item in imageInfos)
            {
                if (imagesCached.Contains(item))
                    imagesCached.Remove(item);
            }

            // Call decache for the ones that aren't cached anymore
            imagesCached.ForEach(
                (item) =>
                {
                    item.Decache();
                }
            );

            GC.Collect();

            // Fill the cache
            imagesCached.Clear();
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
