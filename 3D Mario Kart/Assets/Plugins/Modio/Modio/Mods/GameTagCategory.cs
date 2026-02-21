using System;
using System.Linq;
using System.Threading.Tasks;
using Modio.API.SchemaDefinitions;
using Modio.Extensions;
using Modio.Settings;
using Newtonsoft.Json;

namespace Modio.Mods
{
    public class GameTagCategory
    {
        static GameTagCategory[] _cachedTags;
        static GameTagCategory[] _cachedCollectionTags;
        
        public readonly string Name;
        public readonly bool MultiSelect;
        public readonly ModTag[] Tags; 
        public readonly bool Hidden;
        public readonly bool Locked;

        [JsonConstructor]
        internal GameTagCategory(string name, bool multiSelect, ModTag[] tags, bool hidden, bool locked)
        {
            Name = name;
            MultiSelect = multiSelect;
            Tags = tags;
            Hidden = hidden;
            Locked = locked;
        }
        
        internal GameTagCategory(GameTagOptionObject tagObject){
            Name = tagObject.Name;
            MultiSelect = tagObject.Type == "checkboxes";
            Hidden = tagObject.Hidden;
            Locked = tagObject.Locked;
            Tags = tagObject.Tags.Select(tagName => ModTag.Get(tagName)).ToArray();
            
            foreach ((string tagName, int count) in tagObject.TagCountMap)
            {
                ModTag tag = ModTag.Get(tagName);
                tag.Count = count;
            }

            if (tagObject.TagsLocalization != null)
                foreach (var localization in tagObject.TagsLocalization)
                {
                    ModTag tag = ModTag.Get(localization.Tag);
                    tag.SetLocalizations(localization.Translations);
                }
        }

        static GameTagCategory()
        {
            //handle restarting the plugin and potentially changing game
            ModioClient.OnInitialized += () => _cachedTags = null;
        }

        public static async Task<(Error, GameTagCategory[])> GetGameTagOptions()
        {
            if (_cachedTags != null) return (Error.None, _cachedTags);
            
            (Error error, Pagination<GameTagOptionObject[]>? gameTagOptionObjects) = await API.ModioAPI.Tags.GetGameTagOptions();
            
            if (error)
            {
                (Error readCacheError, GameData cachedGameData) = await GameData.GetGameDataFromDisk();
                
                if (readCacheError)
                {
                    //Note we return the web error, not the cache error, as that's more useful
                    return (error, Array.Empty<GameTagCategory>());
                }
                
                _cachedTags = cachedGameData.Categories;
                return (Error.None, _cachedTags);
            }

            _cachedTags = gameTagOptionObjects.Value.Data.Select(options => new GameTagCategory(options)).ToArray();
            
            GameData.SetGameTags(_cachedTags).ForgetTaskSafely();
            
            return (Error.None, _cachedTags);
        }

        /// <summary>
        /// Get the tags which are usable for collections. These are hardcoded
        /// and not changeable per game.
        /// </summary>
        public static Task<(Error, GameTagCategory[])> GetCollectionTagOptions()
        {
            _cachedCollectionTags ??= new[]
            {
                new GameTagCategory(
                    "Category",
                    false,
                    new[]
                    {
                        ModTag.Get("Miscellaneous", ResourceTagType.CollectionCategory),
                        ModTag.Get("Essential", ResourceTagType.CollectionCategory),
                        ModTag.Get("Themed", ResourceTagType.CollectionCategory),
                    },
                    false,
                    false
                ),
                new GameTagCategory(
                    "Tags",
                    true,
                    new[]
                    {
                        ModTag.Get("Animation", ResourceTagType.CollectionTag),
                        ModTag.Get("Audio", ResourceTagType.CollectionTag),
                        ModTag.Get("Bug Fixes", ResourceTagType.CollectionTag),
                        ModTag.Get("Cheating", ResourceTagType.CollectionTag),
                        ModTag.Get("Environment", ResourceTagType.CollectionTag),
                        ModTag.Get("Gameplay", ResourceTagType.CollectionTag),
                        ModTag.Get("Quality of Life", ResourceTagType.CollectionTag),
                        ModTag.Get("UI", ResourceTagType.CollectionTag),
                    },
                    false,
                    false
                ),
            };
            
            return Task.FromResult((Error.None, _cachedCollectionTags));
        }
    }
}
