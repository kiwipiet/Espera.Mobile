﻿using System;
using System.Collections.Generic;
using System.Linq;
using Espera.Network;

namespace Espera.Mobile.Core
{
    public static class SortHelpers
    {
        public static IEnumerable<T> Order<T>(this IEnumerable<T> songs) where T : NetworkSong
        {
            return songs
                .OrderBy(song => song.Album)
                .ThenBy(song => song.TrackNumber);
        }

        /// <summary>
        /// Removes the prefixes "A", "An" and "The" from the beginning of the artist name.
        /// </summary>
        /// <example>With prefixes "A" and "The": "A Bar" -&gt; "Bar", "The Foos" -&gt; "Foos"</example>
        public static string RemoveArtistPrefixes(string artistName)
        {
            var prefixes = new[] { "A", "An", "The" };
            foreach (string prefix in prefixes)
            {
                int lengthWithSpace = prefix.Length + 1;

                if (artistName.Length >= lengthWithSpace && artistName.Substring(0, lengthWithSpace).Equals(prefix + " ", StringComparison.OrdinalIgnoreCase))
                {
                    return artistName.Substring(lengthWithSpace);
                }
            }

            return artistName;
        }
    }
}