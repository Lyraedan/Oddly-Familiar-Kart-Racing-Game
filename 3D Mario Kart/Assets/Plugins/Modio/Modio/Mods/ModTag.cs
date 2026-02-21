using System.Collections.Generic;
using Modio.API;
using Modio.API.SchemaDefinitions;
using Newtonsoft.Json;

namespace Modio.Mods
{
    public enum ResourceTagType
    {
        ModTag,
        CollectionCategory,
        CollectionTag,
    }
    
    public class ModTag
    {
        static readonly Dictionary<(string, ResourceTagType), ModTag> Tags = new Dictionary<(string, ResourceTagType), ModTag>();

        public readonly string ApiName;
        Dictionary<string, string> _translations;
        public string NameLocalized { get; private set; }
        public bool IsVisible { get; private set; }
        public ResourceTagType TagType { get; }
        public int Count { get; internal set; }

        ModTag(string apiName, ResourceTagType tagType)
        {
            ApiName = apiName;
            TagType = tagType;
        }

        [JsonConstructor]
        public ModTag(string apiName, Dictionary<string, string> translations, string nameLocalized, bool isVisible, int count)
        {
            ApiName = apiName;
            _translations = translations;
            NameLocalized = nameLocalized;
            IsVisible = isVisible;
            Count = count;
        }

        internal static ModTag Get(ModTagObject modTag)
        {
            if (Tags.TryGetValue((modTag.Name, ResourceTagType.ModTag), out ModTag tag))
            {
                //in case we previously cached via apiName only
                tag.NameLocalized = modTag.NameLocalized;
                return tag;
            }
            tag = new ModTag(modTag.Name, ResourceTagType.ModTag) { NameLocalized = modTag.NameLocalized, };
            Tags.Add((modTag.Name, ResourceTagType.ModTag), tag);
            return tag;
        }
        
        internal static ModTag Get(string tagName, ResourceTagType tagType = ResourceTagType.ModTag)
        {
            if (Tags.TryGetValue((tagName, tagType), out ModTag tag))
                return tag;
            tag = new ModTag(tagName, tagType) { NameLocalized = tagName, };//fallback to API name
            Tags.Add((tagName, tagType), tag);
            return tag;
        }

        public void SetLocalizations(Dictionary<string,string> translations)
        {
            _translations = translations;

            if (translations.TryGetValue(ModioAPI.LanguageCodeResponse, out string translation))
                NameLocalized = translation;
        }
    }
}
