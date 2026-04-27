using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTMapEditor.Maps;

namespace TTMapEditor.Saving
{
    /// <summary>
    /// Provides high-level operations for working with map files in the editor.
    /// 
    /// Responsibilities:
    /// - Resolving map file paths (both existing and new maps) relative to the maps root.
    /// - Ensuring required directories exist before accessing map files.
    /// - Creating <see cref="MapPreview"/> instances for existing and new maps.
    /// - Delegating map saving operations to <see cref="MapPreview"/>.
    /// </summary>
    internal class MapEditingMapService
    {

        readonly string mMapsRoot;

        /// <summary>
        /// Initializes a new instance of the <see cref="MapEditingMapService"/> class.
        /// </summary>
        /// <param name="pMapsRoot">
        /// The root directory under which map files are stored. This path is used as the base
        /// for resolving relative map paths and for creating directories when needed.
        /// </param>
        public MapEditingMapService(string pMapsRoot)
        {
            mMapsRoot = pMapsRoot;
        }

        /// <summary>
        /// Creates a <see cref="MapPreview"/> for an existing map file.
        /// </summary>
        /// <param name="pMapFile">
        /// The path or name of the map file. This can be:
        /// - An absolute path to a file or directory.
        /// - A relative path under the maps root.
        /// - A bare file name (with or without extension).
        /// If the value is <c>null</c> or empty, a default <c>map.json</c> under the maps root is used.
        /// </param>
        /// <returns>
        /// A <see cref="MapPreview"/> instance pointing at the resolved map file path.
        /// </returns>
        public MapPreview CreatePreviewForExistingMap(string pMapFile)
        {
            string resolvedPath = ResolveMapPath(pMapFile);
            return new MapPreview(resolvedPath);
        }

        /// <summary>
        /// Creates a <see cref="MapPreview"/> for a new map file.
        /// </summary>
        /// <param name="pMapFile">
        /// The desired path or name for the new map. This can be:
        /// - <c>null</c> / empty to use the maps root.
        /// - A relative name under the maps root.
        /// - An absolute path.
        /// When a directory (or a path without extension) is provided, a <c>map.json</c> file
        /// is assumed within that directory.
        /// </param>
        /// <returns>
        /// A <see cref="MapPreview"/> instance targeting the calculated location for the new map.
        /// </returns>
        public MapPreview CreatePreviewForNewMap(string pMapFile)
        {
            string targetPath = ResolveNewMapPath(pMapFile);
            return new MapPreview(targetPath);
        }

        /// <summary>
        /// Saves the specified map preview using the given map name.
        /// </summary>
        /// <param name="pPreview">The <see cref="MapPreview"/> to save.</param>
        /// <param name="pName">
        /// The logical name of the map. The exact use of this name is delegated to
        /// <see cref="MapPreview.SaveMap(string)"/>.
        /// </param>
        public void SaveMap(MapPreview pPreview, string pName)
        {
            pPreview.SaveMap(pName);
        }

        /// <summary>
        /// Resolves a path for an existing map file, creating any required directories.
        /// 
        /// Behavior:
        /// - If <paramref name="pMapFile"/> is <c>null</c> or empty, uses <c>map.json</c> in <see cref="mMapsRoot"/>.
        /// - If the path is absolute:
        ///     - When it points to a directory or has no extension, creates that directory and uses <c>map.json</c> inside it.
        ///     - Otherwise, treats it as a direct file path.
        /// - If the path is relative:
        ///     - Strips an initial <c>Maps/</c> or <c>Maps\</c> prefix.
        ///     - Resolves under <see cref="mMapsRoot"/>.
        ///     - If the result is a directory or has no extension, ensures the directory exists and uses <c>map.json</c>.
        ///     - Otherwise, ensures the parent directory exists and uses the specified file name.
        /// </summary>
        /// <param name="pMapFile">User-supplied map path or name.</param>
        /// <returns>Full path to the map file to be used.</returns>
        string ResolveMapPath(string pMapFile)
        {
            if (string.IsNullOrEmpty(pMapFile))
            {
                Directory.CreateDirectory(mMapsRoot);
                return Path.GetFullPath(Path.Combine(mMapsRoot, "map.json"));
            }

            if (Path.IsPathRooted(pMapFile))
            {
                if (Directory.Exists(pMapFile) || !Path.HasExtension(pMapFile))
                {
                    Directory.CreateDirectory(pMapFile);
                    return Path.GetFullPath(Path.Combine(pMapFile, "map.json"));
                }

                return Path.GetFullPath(pMapFile);
            }

            string relative = NormalizeMapsPrefix(pMapFile);

            string candidate = Path.Combine(mMapsRoot, relative);

            if (Directory.Exists(candidate) || !Path.HasExtension(candidate))
            {
                Directory.CreateDirectory(candidate);
                return Path.GetFullPath(Path.Combine(candidate, "map.json"));
            }

            string? dir = Path.GetDirectoryName(candidate);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }

            return Path.GetFullPath(candidate);
        }

        /// <summary>
        /// Resolves the target file path for a new map without creating directories on disk.
        /// 
        /// Behavior:
        /// - Normalizes an initial <c>Maps/</c> or <c>Maps\</c> prefix.
        /// - If the resulting value is empty, uses the maps root.
        /// - If the path is absolute or already contains directory separators, uses it as-is.
        /// - Otherwise, combines it with the maps root.
        /// - When the resolved path is an existing directory or has no extension, returns
        ///   a path to <c>map.json</c> within that directory; otherwise returns the path itself.
        /// </summary>
        /// <param name="pMapFile">Desired path or name for the new map.</param>
        /// <returns>Full path where the new map file should be created.</returns>
        string ResolveNewMapPath(string pMapFile)
        {
            string mapsRoot = mMapsRoot;

            string relative = pMapFile ?? string.Empty;
            relative = NormalizeMapsPrefix(relative);

            string candidate;
            if (string.IsNullOrWhiteSpace(relative))
            {
                candidate = mapsRoot;
            }
            else if (Path.IsPathRooted(relative) || relative.Contains(Path.DirectorySeparatorChar) || relative.Contains(Path.AltDirectorySeparatorChar))
            {
                candidate = relative;
            }
            else
            {
                candidate = Path.Combine(mapsRoot, relative);
            }

            string targetPath;
            if (Directory.Exists(candidate) || !Path.HasExtension(candidate))
            {
                targetPath = Path.Combine(candidate, "map.json");
            }
            else
            {
                targetPath = candidate;
            }

            return Path.GetFullPath(targetPath);
        }

        /// <summary>
        /// Removes a leading <c>Maps/</c> or <c>Maps\</c> prefix from the given path, if present.
        /// This allows callers to pass editor-style paths starting with the logical
        /// &quot;Maps&quot; root without duplicating the maps root on disk.
        /// </summary>
        /// <param name="pPath">The original path, possibly starting with a &quot;Maps&quot; segment.</param>
        /// <returns>The path without the leading &quot;Maps&quot; segment, or the original path if no match.</returns>
        static string NormalizeMapsPrefix(string pPath)
        {
            string relative = pPath;
            string mapsPrefix1 = "Maps" + Path.DirectorySeparatorChar;
            string mapsPrefix2 = "Maps" + Path.AltDirectorySeparatorChar;

            if (relative.StartsWith(mapsPrefix1) || relative.StartsWith(mapsPrefix2))
            {
                relative = relative.Substring(5);
            }

            return relative;
        }
    }
}
