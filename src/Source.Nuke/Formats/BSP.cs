using Nuke.Common.Utilities.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ValveKeyValue;
namespace Nuke.Common.Tools.Source.Formats
{
    // this is the class that stores data about the bsp.
    // You can find information about the file format here
    // https://developer.valvesoftware.com/wiki/Source_BSP_File_Format#BSP_file_header

    internal class BSP
    {
        private FileStream bsp;
        private BinaryReader reader;
        private KeyValuePair<int, int>[] offsets; // offset/length
        private static readonly char[] SpecialCaracters = { '*', '#', '@', '>', '<', '^', '(', ')', '}', '$', '!', '?', ' ' };

        public List<Dictionary<string, string>> entityList { get; private set; }

        public List<List<Tuple<string, string>>> entityListArrayForm { get; private set; }

        public List<int>[] modelSkinList { get; private set; }

        public List<string> ModelList { get; private set; }

        public List<string> EntModelList { get; private set; }

        public List<string> ParticleList { get; private set; }

        public List<string> TextureList { get; private set; }
        public List<string> EntTextureList { get; private set; }

        public List<string> EntSoundList { get; private set; }

        public List<string> MiscList { get; private set; }

        // key/values as internalPath/externalPath
        public KeyValuePair<string, string> particleManifest { get; set; }
        public KeyValuePair<string, string> soundscript { get; set; }
        public KeyValuePair<string, string> soundscape { get; set; }
        public KeyValuePair<string, string> detail { get; set; }
        public KeyValuePair<string, string> nav { get; set; }
        public List<KeyValuePair<string, string>> res { get; } = new List<KeyValuePair<string, string>>();
        public KeyValuePair<string, string> kv { get; set; }
        public KeyValuePair<string, string> txt { get; set; }
        public KeyValuePair<string, string> jpg { get; set; }
        public KeyValuePair<string, string> radartxt { get; set; }
        public List<KeyValuePair<string, string>> radardds { get; set; }
        public KeyValuePair<string, string> RadarTablet { get; set; }
        public List<KeyValuePair<string, string>> languages { get; set; }
        public List<KeyValuePair<string, string>> VehicleScriptList { get; set; }
        public List<KeyValuePair<string, string>> EffectScriptList { get; set; }
        public List<string> vscriptList { get; set; }
        public List<KeyValuePair<string, string>> PanoramaMapBackgrounds { get; set; }
        public KeyValuePair<string, string> PanoramaMapIcon { get; set; }

        public bool Verbose { get; set; }
        public string GameFolder { get; private set; }

        public FileInfo File { get; private set; }

        private bool isL4D2 = false;
        private int bspVersion;

        public BSP(FileInfo file, string gameFolder)
        {
            this.File = file;
            this.GameFolder = gameFolder;

            offsets = new KeyValuePair<int, int>[64];
            using (bsp = new FileStream(file.FullName, FileMode.Open))
            using (reader = new BinaryReader(bsp))
            {
                bsp.Seek(4, SeekOrigin.Begin); //skip header
                this.bspVersion = reader.ReadInt32();

                //hack for detecting l4d2 maps
                if (reader.ReadInt32() == 0 && this.bspVersion == 21)
                    isL4D2 = true;

                // reset reader position
                bsp.Seek(-4, SeekOrigin.Current);

                //gathers an array of offsets (where things are located in the bsp)
                for (var i = 0; i < offsets.GetLength(0); i++)
                {
                    // l4d2 has different lump order
                    if (isL4D2)
                    {
                        bsp.Seek(4, SeekOrigin.Current); //skip version
                        offsets[i] = new KeyValuePair<int, int>(reader.ReadInt32(), reader.ReadInt32());
                        bsp.Seek(4, SeekOrigin.Current); //skip id
                    }
                    else
                    {
                        offsets[i] = new KeyValuePair<int, int>(reader.ReadInt32(), reader.ReadInt32());
                        bsp.Seek(8, SeekOrigin.Current); //skip id and version
                    }
                }

                buildEntityList();

                buildEntModelList();
                buildModelList();

                buildParticleList();

                buildEntTextureList();
                buildTextureList();

                buildEntSoundList();

                buildMiscList();
            }
        }

        private void buildEntityList()
        {
            entityList = new List<Dictionary<string, string>>();
            entityListArrayForm = new List<List<Tuple<string, string>>>();

            bsp.Seek(offsets[0].Key, SeekOrigin.Begin);
            var ent = reader.ReadBytes(offsets[0].Value);
            var ents = new List<byte>();

            const int LCURLY = 123;
            const int RCURLY = 125;
            const int NEWLINE = 10;

            for (var i = 0; i < ent.Length; i++)
            {
                if (ent[i] == LCURLY && i + 1 < ent.Length)
                {
                    // if curly isnt followed by newline assume its part of filename
                    if (ent[i + 1] != NEWLINE)
                        ents.Add(ent[i]);
                }
                if (ent[i] != LCURLY && ent[i] != RCURLY)
                    ents.Add(ent[i]);
                else if (ent[i] == RCURLY)
                {
                    // if curly isnt followed by newline assume its part of filename
                    if (i + 1 < ent.Length && ent[i + 1] != NEWLINE)
                    {
                        ents.Add(ent[i]);
                        continue;
                    }


                    var rawent = Encoding.ASCII.GetString(ents.ToArray());
                    var entity = new Dictionary<string, string>();
                    var entityArrayFormat = new List<Tuple<string, string>>();
                    // split on \n, ignore \n inside of quotes
                    foreach (var s in Regex.Split(rawent, "(?=(?:(?:[^\"]*\"){2})*[^\"]*$)\\n"))
                    {
                        if (s.Count() != 0)
                        {
                            var c = s.Split('"');
                            if (!entity.ContainsKey(c[1]))
                                entity.Add(c[1], c[3]);
                            entityArrayFormat.Add(Tuple.Create(c[1], c[3]));
                        }
                    }
                    entityList.Add(entity);
                    entityListArrayForm.Add(entityArrayFormat);
                    ents = new List<byte>();
                }
            }
        }

        private void buildTextureList()
        {
            // builds the list of textures applied to brushes

            var mapname = bsp.Name.Split('\\').Last().Split('.')[0];

            TextureList = new List<string>();
            bsp.Seek(offsets[43].Key, SeekOrigin.Begin);
            TextureList = new List<string>(Encoding.ASCII.GetString(reader.ReadBytes(offsets[43].Value)).Split('\0'));
            for (var i = 0; i < TextureList.Count; i++)
            {
                if (TextureList[i].StartsWith("/")) // materials in root level material directory start with /
                    TextureList[i] = "materials" + TextureList[i] + ".vmt";
                else
                    TextureList[i] = "materials/" + TextureList[i] + ".vmt";
            }

            // find skybox materials
            var worldspawn = entityList.First(item => item["classname"] == "worldspawn");
            if (worldspawn.ContainsKey("skyname"))
                foreach (var s in new string[] { "", "bk", "dn", "ft", "lf", "rt", "up" })
                {
                    TextureList.Add("materials/skybox/" + worldspawn["skyname"] + s + ".vmt");
                    TextureList.Add("materials/skybox/" + worldspawn["skyname"] + "_hdr" + s + ".vmt");
                }

            // find detail materials
            if (worldspawn.ContainsKey("detailmaterial"))
                TextureList.Add("materials/" + worldspawn["detailmaterial"] + ".vmt");

            // find menu photos
            TextureList.Add("materials/vgui/maps/menu_photos_" + mapname + ".vmt");
        }

        private void buildEntTextureList()
        {
            // builds the list of textures referenced in entities

            var materials = new List<string>();
            var skybox_swappers = new HashSet<string>();

            foreach (var ent in entityList)
            {
                foreach (var prop in ent)
                {
                    if (Keys.vmfMaterialKeys.Contains(prop.Key.ToLower()))
                    {
                        materials.Add(prop.Value);
                        if (prop.Key.ToLower().StartsWith("team_icon"))
                            materials.Add(prop.Value + "_locked");
                    }

                }

                if (ent["classname"].Contains("skybox_swapper") && ent.ContainsKey("SkyboxName"))
                {
                    if (ent.ContainsKey("targetname"))
                    {
                        skybox_swappers.Add(ent["targetname"].ToLower());
                    }

                    foreach (var s in new string[] { "", "bk", "dn", "ft", "lf", "rt", "up" })
                    {
                        materials.Add("skybox/" + ent["SkyboxName"] + s + ".vmt");
                        materials.Add("skybox/" + ent["SkyboxName"] + "_hdr" + s + ".vmt");
                    }
                }

                // special condition for sprites
                if (ent["classname"].Contains("sprite") && ent.ContainsKey("model"))
                    materials.Add(ent["model"]);

                // special condition for item_teamflag
                if (ent["classname"].Contains("item_teamflag"))
                {
                    if (ent.ContainsKey("flag_trail"))
                    {
                        materials.Add("effects/" + ent["flag_trail"]);
                        materials.Add("effects/" + ent["flag_trail"] + "_red");
                        materials.Add("effects/" + ent["flag_trail"] + "_blu");
                    }
                    if (ent.ContainsKey("flag_icon"))
                    {
                        materials.Add("vgui/" + ent["flag_icon"]);
                        materials.Add("vgui/" + ent["flag_icon"] + "_red");
                        materials.Add("vgui/" + ent["flag_icon"] + "_blu");
                    }
                }

                // special condition for env_funnel. Hardcoded to use sprites/flare6.vmt
                if (ent["classname"].Contains("env_funnel"))
                    materials.Add("sprites/flare6.vmt");

                // special condition for env_embers. Hardcoded to use particle/fire.vmt
                if (ent["classname"].Contains("env_embers"))
                    materials.Add("particle/fire.vmt");

                //special condition for func_dustcloud and func_dustmotes.  Hardcoded to use particle/sparkles.vmt
                if (ent["classname"].StartsWith("func_dust"))
                    materials.Add("particle/sparkles.vmt");

                // special condition for vgui_slideshow_display. directory paramater references all textures in a folder (does not include subfolders)
                if (ent["classname"].Contains("vgui_slideshow_display"))
                {
                    if (ent.ContainsKey("directory"))
                    {
                        var directory = $"{GameFolder}/materials/vgui/{ent["directory"]}";
                        if (Directory.Exists(directory))
                        {
                            foreach (var file in Directory.GetFiles(directory))
                            {
                                if (file.EndsWith(".vmt"))
                                    materials.Add($"/vgui/{ent["directory"]}/{Path.GetFileName(file)}");
                            }
                        }
                    }
                }

            }

            // pack I/O referenced materials
            // need to use array form of entity because multiple outputs with same command can't be stored in dict
            foreach (var ent in entityListArrayForm)
            {
                foreach (var prop in ent)
                {
                    var io = ParseIO(prop.Item2);
                    if (io == null)
                        continue;


                    var (target, command, parameter) = io;

                    switch (command)
                    {
                        case "SetCountdownImage":
                            materials.Add($"vgui/{parameter}");
                            break;
                        case "Command":
                            // format of Command is <command> <parameter>
                            if (!parameter.Contains(' '))
                            {
                                continue;
                            }
                            (command, parameter) = parameter.Split(' ') switch { var param => (param[0], param[1]) };
                            if (command == "r_screenoverlay")
                                materials.Add(parameter);
                            break;
                        case "AddOutput":
                            if (!parameter.Contains(' '))
                            {
                                continue;
                            }
                            string k, v;
                            (k, v) = parameter.Split(' ') switch { var a => (a[0], a[1]) };

                            // support packing mats when using addoutput to change skybox_swappers
                            if (skybox_swappers.Contains(target.ToLower()) && k.ToLower() == "skyboxname")
                            {
                                foreach (var s in new string[] { "", "bk", "dn", "ft", "lf", "rt", "up" })
                                {
                                    materials.Add("skybox/" + v + s + ".vmt");
                                    materials.Add("skybox/" + v + "_hdr" + s + ".vmt");
                                }
                            }
                            break;
                    }
                }
            }

            // format and add materials
            EntTextureList = new List<string>();
            foreach (var material in materials)
            {
                var materialpath = material;
                if (!material.EndsWith(".vmt") && !materialpath.EndsWith(".spr"))
                    materialpath += ".vmt";

                EntTextureList.Add("materials/" + materialpath);
            }
        }

        private void buildModelList()
        {
            // builds the list of models that are from prop_static

            ModelList = new List<string>();
            // getting information on the gamelump
            var propStaticId = 0;
            bsp.Seek(offsets[35].Key, SeekOrigin.Begin);
            var GameLumpOffsets = new KeyValuePair<int, int>[reader.ReadInt32()]; // offset/length
            for (var i = 0; i < GameLumpOffsets.Length; i++)
            {
                if (reader.ReadInt32() == 1936749168)
                    propStaticId = i;
                bsp.Seek(4, SeekOrigin.Current); //skip flags and version
                GameLumpOffsets[i] = new KeyValuePair<int, int>(reader.ReadInt32(), reader.ReadInt32());
            }

            // reading model names from game lump
            bsp.Seek(GameLumpOffsets[propStaticId].Key, SeekOrigin.Begin);
            var modelCount = reader.ReadInt32();
            for (var i = 0; i < modelCount; i++)
            {
                var model = Encoding.ASCII.GetString(reader.ReadBytes(128)).Trim('\0');
                if (model.Length != 0)
                    ModelList.Add(model);
            }

            // from now on we have models, now we want to know what skins they use

            // skipping leaf lump
            var leafCount = reader.ReadInt32();

            // bsp v25 uses ints instead of shorts for leaf lump
            if (this.bspVersion == 25)
                bsp.Seek(leafCount * sizeof(int), SeekOrigin.Current);
            else
                bsp.Seek(leafCount * sizeof(short), SeekOrigin.Current);

            // reading staticprop lump

            var propCount = reader.ReadInt32();

            //dont bother if there's no props, avoid a dividebyzero exception.
            if (propCount <= 0)
                return;

            var propOffset = bsp.Position;
            var byteLength = GameLumpOffsets[propStaticId].Key + GameLumpOffsets[propStaticId].Value - (int)propOffset;
            var propLength = byteLength / propCount;

            modelSkinList = new List<int>[modelCount]; // stores the ids of used skins

            for (var i = 0; i < modelCount; i++)
                modelSkinList[i] = new List<int>();

            for (var i = 0; i < propCount; i++)
            {
                bsp.Seek(i * propLength + propOffset + 24, SeekOrigin.Begin); // 24 skips origin and angles
                int modelId = reader.ReadUInt16();
                bsp.Seek(6, SeekOrigin.Current);
                var skin = reader.ReadInt32();

                if (modelSkinList[modelId].IndexOf(skin) == -1)
                    modelSkinList[modelId].Add(skin);
            }

        }

        private void buildEntModelList()
        {
            // builds the list of models referenced in entities

            EntModelList = new List<string>();
            foreach (var ent in entityList)
            {
                foreach (var prop in ent)
                {
                    if (ent["classname"].StartsWith("func"))
                    {
                        if (prop.Key == "gibmodel")
                            EntModelList.Add(prop.Value);
                    }
                    else if (!ent["classname"].StartsWith("trigger") &&
                             !ent["classname"].Contains("sprite"))
                    {
                        if (Keys.vmfModelKeys.Contains(prop.Key))
                            EntModelList.Add(prop.Value);
                        // item_sodacan is hardcoded to models/can.mdl
                        // env_beverage spawns item_sodacans
                        else if (prop.Value == "item_sodacan" || prop.Value == "env_beverage")
                            EntModelList.Add("models/can.mdl");
                        // tf_projectile_throwable is hardcoded to models/props_gameplay/small_loaf.mdl
                        else if (prop.Value == "tf_projectile_throwable")
                            EntModelList.Add("models/props_gameplay/small_loaf.mdl");
                    }

                }
            }

            // pack I/O referenced models
            // need to use array form of entity because multiple outputs with same command can't be stored in dict
            foreach (var ent in entityListArrayForm)
            {
                foreach (var prop in ent)
                {
                    var io = ParseIO(prop.Item2);
                    if (io == null)
                        continue;

                    var (target, command, parameter) = io;

                    if (command == "SetModel")
                        EntModelList.Add(parameter);
                }
            }
        }

        private void buildEntSoundList()
        {
            // builds the list of sounds referenced in entities
            EntSoundList = new List<string>();
            foreach (var ent in entityList)
            foreach (var prop in ent)
            {
                if (Keys.vmfSoundKeys.Contains(prop.Key.ToLower()))
                    EntSoundList.Add("sound/" + prop.Value.Trim(SpecialCaracters));
            }

            // pack I/O referenced sounds
            // need to use array form of entity because multiple outputs with same command can't be stored in dict
            foreach (var ent in entityListArrayForm)
            {
                foreach (var prop in ent)
                {
                    var io = ParseIO(prop.Item2);
                    if (io == null)
                        continue;

                    var (target, command, parameter) = io;
                    if (command == "PlayVO")
                    {
                        //Parameter value following PlayVO is always either a sound path or an empty string
                        if (!string.IsNullOrWhiteSpace(parameter))
                            EntSoundList.Add($"sound/{parameter}");
                    }
                    else if (command == "Command")
                    {
                        // format of Command is <command> <parameter>
                        if (!parameter.Contains(' '))
                            continue;

                        (command, parameter) = parameter.Split(' ') switch { var param => (param[0], param[1]) };

                        if (command == "play" || command == "playgamesound")
                            EntSoundList.Add($"sound/{parameter}");
                    }
                }
            }
        }
        // color correction, etc.
        private void buildMiscList()
        {
            MiscList = new List<string>();

            // find color correction files
            foreach (var cc in entityList.Where(item => item["classname"].StartsWith("color_correction")))
                if (cc.ContainsKey("filename"))
                    TextureList.Add(cc["filename"]);

            // pack I/O referenced TF2 upgrade files
            // need to use array form of entity because multiple outputs with same command can't be stored in dict
            foreach (var ent in entityListArrayForm)
            {
                foreach (var prop in ent)
                {
                    var io = ParseIO(prop.Item2);

                    if (io == null) continue;

                    var (target, command, parameter) = io;
                    if (command.ToLower() != "setcustomupgradesfile") continue;

                    MiscList.Add(parameter);

                }
            }
        }

        private void buildParticleList()
        {
            ParticleList = new List<string>();
            foreach (var ent in entityList)
            foreach (var particle in ent)
                if (particle.Key.ToLower() == "effect_name")
                    ParticleList.Add(particle.Value);
        }

        /// <summary>
        /// Parses an IO string for the command and parameter. If the command is "AddOutput", it is parsed returns target, command, parameter
        /// </summary>
        /// <param name="property">Entity property</param>
        /// <returns>Tuple containing (target, command, parameter)</returns>
        private Tuple<string, string, string>? ParseIO(string property)
        {
            // io is split by unicode escape char
            if (!property.Contains("\u001b"))
            {
                return null;
            }

            // format: <target>\u001b<target input>\u001b<parameter>\u001b<delay>\u001b<only once>
            var io = property.Split("\u001b");
            if (io.Length != 5)
            {
                System.Diagnostics.Trace.TraceWarning($"Failed to decode IO, ignoring: {property}");
                return null;
            }

            var targetInput = io[1];
            var parameter = io[2];

            // AddOutput dynamically adds I/O to other entities, parse it to get input/parameter
            if (targetInput == "AddOutput")
            {
                // AddOutput format: <output name> <target name>:<input name>:<parameter>:<delay>:<max times to fire> or simple form <key> <value>
                // only need to convert complex form into simple form
                if (parameter.Contains(':'))
                {
                    var splitIo = parameter.Split(':');
                    if (splitIo.Length < 3)
                    {
                        System.Diagnostics.Trace.TraceWarning($"Failed to decode AddOutput, format may be incorrect: {property}");
                        return null;
                    }

                    targetInput = splitIo[1];
                    parameter = splitIo[2];
                }
            }

            return new Tuple<string, string, string>(io[0], targetInput, parameter);
        }

        private List<string> getSourceDirectories(string depotDirectory, long appId, string gameName)
        {
            var gameDirectory = Path.Combine(depotDirectory, appId.ToString(), gameName);

            var sourceDirectories = new List<string>();
            var gameInfoPath = Path.Combine(gameDirectory, "gameinfo.txt");
            var rootPath = Directory.GetParent(gameDirectory).ToString();

            if (!System.IO.File.Exists(gameInfoPath))
            {
                Trace.TraceError($"Couldn't find gameinfo.txt at {gameInfoPath}");
                return new();
            }

            var fileStream = System.IO.File.OpenRead(gameInfoPath);
            var gameInfo = KVSerializer.Create(KVSerializationFormat.KeyValues1Text).Deserialize(fileStream);

            //var gameInfo = new KV.FileData(gameInfoPath).headnode.GetFirstByName("GameInfo");
            if (gameInfo == null)
            {
                Trace.TraceInformation($"Failed to parse GameInfo: {gameInfo}");
                Trace.TraceError($"Failed to parse GameInfo, did not find GameInfo block");
                return new();
            }

            //var searchPaths = gameInfo.GetFirstByName("FileSystem")?.GetFirstByName("SearchPaths");
            var searchPaths = gameInfo["FileSystem"]?["SearchPaths"];
            if (searchPaths == null)
            {
                Trace.TraceInformation($"Failed to parse GameInfo: {gameInfo}");
                Trace.TraceError($"Failed to parse GameInfo, did not find GameInfo block");
                return new();
            }

            var collection = searchPaths.AsEnumerable<KVObject>().Select(m => new KeyValuePair<string, string>(m.Name, m.Value.ToString()));
            foreach (var searchPath in collection)
            {
                // ignore unsearchable paths. TODO: will need to remove .vpk from this check if we add support for packing from assets within vpk files
                if (searchPath.Value.Contains("|") && !searchPath.Value.Contains("|gameinfo_path|") || searchPath.Value.Contains(".vpk")) continue;

                // wildcard paths
                if (searchPath.Value.Contains("*"))
                {
                    var fullPath = searchPath.Value;
                    if (fullPath.Contains(("|gameinfo_path|")))
                    {
                        var newPath = searchPath.Value.Replace("*", "").Replace("|gameinfo_path|", "");
                        fullPath = Path.GetFullPath(gameDirectory + "\\" + newPath.TrimEnd('\\'));
                    }
                    if (Path.IsPathRooted(fullPath.Replace("*", "")))
                    {
                        fullPath = fullPath.Replace("*", "");
                    }
                    else
                    {
                        var newPath = fullPath.Replace("*", "");
                        fullPath = Path.GetFullPath(rootPath + "\\" + newPath.TrimEnd('\\'));
                    }

                    if (Verbose)
                        Trace.TraceInformation("Found wildcard path: {0}", fullPath);

                    try
                    {
                        var directories = Directory.GetDirectories(fullPath);
                        sourceDirectories.AddRange(directories);
                    }
                    catch { }
                }
                else if (searchPath.Value.Contains("|gameinfo_path|"))
                {
                    var fullPath = gameDirectory;

                    if (Verbose)
                        Trace.TraceInformation("Found search path: {0}", fullPath);

                    sourceDirectories.Add(fullPath);
                }
                else if (Directory.Exists(searchPath.Value))
                {
                    if (Verbose)
                        Trace.TraceInformation("Found search path: {0}", searchPath);

                    sourceDirectories.Add(searchPath.Value);
                }
                else
                {
                    try
                    {
                        var fullPath = Path.GetFullPath(rootPath + "\\" + searchPath.Value.TrimEnd('\\'));

                        if (Verbose)
                            Trace.TraceInformation("Found search path: {0}", fullPath);

                        sourceDirectories.Add(fullPath);
                    }
                    catch (Exception e)
                    {
                        Trace.TraceInformation("Failed to find search path: " + e);
                        Trace.TraceWarning($"Search path invalid: {rootPath + "\\" + searchPath.Value.TrimEnd('\\')}");
                    }
                }
            }

            // find Chaos engine game mount paths
            // var mountedDirectories = GetMountedGamesSourceDirectories(gameInfo, Path.Combine(GameDirectory, "cfg", "mounts.kv"));
            // if (mountedDirectories != null)
            // {
            //     sourceDirectories.AddRange(mountedDirectories);
            //     foreach (var directory in mountedDirectories)
            //     {
            //         Trace.TraceInformation($"Found mounted search path: {directory}");
            //     }
            // }

            return sourceDirectories.Distinct().ToList();
        }

        internal void findBspUtilityFiles(string depotDirectory, long appId, string gameName)
        {
            var sourceDirectories = getSourceDirectories(Path.GetFullPath(depotDirectory), appId, gameName);
            // Utility files are other files that are not assets and are sometimes not referenced in the bsp
            // those are manifests, soundscapes, nav, radar and detail files

            // Soundscape file
            string internalPath = "scripts/soundscapes_" + File.Name.Replace(".bsp", ".txt");
            // Soundscapes can have either .txt or .vsc extensions
            string internalPathVsc = "scripts/soundscapes_" + File.Name.Replace(".bsp", ".vsc");
            foreach (string source in sourceDirectories)
            {
                string externalPath = source + "/" + internalPath;
                string externalVscPath = source + "/" + internalPathVsc;

                if (System.IO.File.Exists(externalPath))
                {
                    soundscape = new KeyValuePair<string, string>(internalPath, externalPath);
                    break;
                }
                if (System.IO.File.Exists(externalVscPath))
                {
                    soundscape = new KeyValuePair<string, string>(internalPathVsc, externalVscPath);
                    break;
                }
            }

            // Soundscript file
            internalPath = "maps/" + File.Name.Replace(".bsp", "") + "_level_sounds.txt";
            foreach (string source in sourceDirectories)
            {
                string externalPath = source + "/" + internalPath;

                if (System.IO.File.Exists(externalPath))
                {
                    soundscript = new KeyValuePair<string, string>(internalPath, externalPath);
                    break;
                }
            }

            // Nav file (.nav)
            internalPath = "maps/" + File.Name.Replace(".bsp", ".nav");
            foreach (string source in sourceDirectories)
            {
                string externalPath = source + "/" + internalPath;

                if (System.IO.File.Exists(externalPath))
                {
                    if (RenameNav)
                        internalPath = "maps/embed.nav";
                    nav = new KeyValuePair<string, string>(internalPath, externalPath);
                    break;
                }
            }

            // detail file (.vbsp)
            Dictionary<string, string> worldspawn = entityList.First(item => item["classname"] == "worldspawn");
            if (worldspawn.ContainsKey("detailvbsp"))
            {
                internalPath = worldspawn["detailvbsp"];

                foreach (string source in sourceDirectories)
                {
                    string externalPath = source + "/" + internalPath;

                    if (System.IO.File.Exists(externalPath))
                    {
                        detail = new KeyValuePair<string, string>(internalPath, externalPath);
                        break;
                    }
                }
            }


            // Vehicle scripts
            List<KeyValuePair<string, string>> vehicleScripts = new List<KeyValuePair<string, string>>();
            foreach (Dictionary<string, string> ent in entityList)
            {
                if (ent.ContainsKey("vehiclescript"))
                {
                    foreach (string source in sourceDirectories)
                    {
                        string externalPath = source + "/" + ent["vehiclescript"];
                        if (System.IO.File.Exists(externalPath))
                        {
                            internalPath = ent["vehiclescript"];
                            vehicleScripts.Add(new KeyValuePair<string, string>(ent["vehiclescript"], externalPath));
                        }
                    }
                }
            }
            VehicleScriptList = vehicleScripts;

            // Effect Scripts
            List<KeyValuePair<string, string>> effectScripts = new List<KeyValuePair<string, string>>();
            foreach (Dictionary<string, string> ent in entityList)
            {
                if (ent.ContainsKey("scriptfile"))
                {
                    foreach (string source in sourceDirectories)
                    {
                        string externalPath = source + "/" + ent["scriptfile"];
                        if (System.IO.File.Exists(externalPath))
                        {
                            internalPath = ent["scriptfile"];
                            effectScripts.Add(new KeyValuePair<string, string>(ent["scriptfile"], externalPath));
                        }
                    }
                }
            }
            EffectScriptList = effectScripts;

            // Res file (for tf2's pd gamemode)
            Dictionary<string, string>? pd_ent = entityList.FirstOrDefault(item => item["classname"] == "tf_logic_player_destruction");
            if (pd_ent != null && pd_ent.ContainsKey("res_file"))
            {
                foreach (string source in sourceDirectories)
                {
                    string externalPath = source + "/" + pd_ent["res_file"];
                    if (System.IO.File.Exists(externalPath))
                    {
                        res.Add(new KeyValuePair<string, string>(pd_ent["res_file"], externalPath));
                        break;
                    }
                }
            }

            // tf2 tc round overview files
            internalPath = "resource/roundinfo/" + File.Name.Replace(".bsp", ".res");
            foreach (string source in sourceDirectories)
            {
                string externalPath = source + "/" + internalPath;

                if (System.IO.File.Exists(externalPath))
                {
                    res.Add(new KeyValuePair<string, string>(internalPath, externalPath));
                    break;
                }
            }
            internalPath = "materials/overviews/" + File.Name.Replace(".bsp", ".vmt");
            foreach (string source in sourceDirectories)
            {
                string externalPath = source + "/" + internalPath;

                if (System.IO.File.Exists(externalPath))
                {
                    TextureList.Add(internalPath);
                    break;
                }
            }

            // Radar file
            internalPath = "resource/overviews/" + File.Name.Replace(".bsp", ".txt");
            List<KeyValuePair<string, string>> ddsfiles = new List<KeyValuePair<string, string>>();
            foreach (string source in sourceDirectories)
            {
                string externalPath = source + "/" + internalPath;

                if (System.IO.File.Exists(externalPath))
                {
                    radartxt = new KeyValuePair<string, string>(internalPath, externalPath);
                    TextureList.AddRange(findVmtMaterials(externalPath));

                    List<string> ddsInternalPaths = findRadarDdsFiles(externalPath);
                    //find out if they exists or not
                    foreach (string ddsInternalPath in ddsInternalPaths)
                    {
                        foreach (string source2 in sourceDirectories)
                        {
                            string ddsExternalPath = source2 + "/" + ddsInternalPath;
                            if (System.IO.File.Exists(ddsExternalPath))
                            {
                                ddsfiles.Add(new KeyValuePair<string, string>(ddsInternalPath, ddsExternalPath));
                                break;
                            }
                        }
                    }
                    break;
                }
            }
            radardds = ddsfiles;

            // csgo kv file (.kv)
            internalPath = "maps/" + File.Name.Replace(".bsp", ".kv");
            foreach (string source in sourceDirectories)
            {
                string externalPath = source + "/" + internalPath;

                if (System.IO.File.Exists(externalPath))
                {
                    kv = new KeyValuePair<string, string>(internalPath, externalPath);
                    break;
                }
            }

            // csgo loading screen text file (.txt)
            internalPath = "maps/" + File.Name.Replace(".bsp", ".txt");
            foreach (string source in sourceDirectories)
            {
                string externalPath = source + "/" + internalPath;

                if (System.IO.File.Exists(externalPath))
                {
                    txt = new KeyValuePair<string, string>(internalPath, externalPath);
                    break;
                }
            }

            // csgo loading screen image (.jpg)
            internalPath = "maps/" + File.Name.Replace(".bsp", "");
            foreach (string source in sourceDirectories)
            {
                string externalPath = source + "/" + internalPath;
                foreach (string extension in new[] { ".jpg", ".jpeg" })
                    if (System.IO.File.Exists(externalPath + extension))
                        jpg = new KeyValuePair<string, string>(internalPath + ".jpg", externalPath + extension);
            }

            // csgo panorama map backgrounds (.png)
            internalPath = "materials/panorama/images/map_icons/screenshots/";
            var panoramaMapBackgrounds = new List<KeyValuePair<string, string>>();
            foreach (string source in sourceDirectories)
            {
                string externalPath = source + "/" + internalPath;
                string bspName = File.Name.Replace(".bsp", "");

                foreach (string resolution in new[] { "360p", "1080p" })
                    if (System.IO.File.Exists($"{externalPath}{resolution}/{bspName}.png"))
                        panoramaMapBackgrounds.Add(new KeyValuePair<string, string>($"{internalPath}{resolution}/{bspName}.png", $"{externalPath}{resolution}/{bspName}.png"));
            }
            PanoramaMapBackgrounds = panoramaMapBackgrounds;

            // csgo panorama map icon
            internalPath = "materials/panorama/images/map_icons/";
            foreach (string source in sourceDirectories)
            {
                string externalPath = source + "/" + internalPath;
                string bspName = File.Name.Replace(".bsp", "");
                foreach (string extension in new[] { ".svg" })
                    if (System.IO.File.Exists($"{externalPath}map_icon_{bspName}{extension}"))
                        PanoramaMapIcon = new KeyValuePair<string, string>($"{internalPath}map_icon_{bspName}{extension}", $"{externalPath}map_icon_{bspName}{extension}");
            }

            // csgo dz tablets
            internalPath = "materials/models/weapons/v_models/tablet/tablet_radar_" + File.Name.Replace(".bsp", ".vtf");
            foreach (string source in sourceDirectories)
            {
                string externalPath = source + "/" + internalPath;

                if (System.IO.File.Exists(externalPath))
                {
                    RadarTablet = new KeyValuePair<string, string>(internalPath, externalPath);
                    break;
                }
            }

            // language files, particle manifests and soundscript file
            // (these language files are localized text files for tf2 mission briefings)
            string internalDir = "maps/";
            string name = File.Name.Replace(".bsp", "");
            string searchPattern = name + "*.txt";
            List<KeyValuePair<string, string>> langfiles = new List<KeyValuePair<string, string>>();

            foreach (string source in sourceDirectories)
            {
                string externalDir = source + "/" + internalDir;
                DirectoryInfo dir = new DirectoryInfo(externalDir);

                if (dir.Exists)
                    foreach (FileInfo f in dir.GetFiles(searchPattern))
                    {
                        // particle files if particle manifest is not being generated
                        if (f.Name.StartsWith(name + "_particles") || f.Name.StartsWith(name + "_manifest"))
                        {
                            if (!GenParticleManifest)
                                particleManifest = new KeyValuePair<string, string>(internalDir + f.Name, externalDir + f.Name);
                            continue;
                        }
                        // soundscript
                        if (f.Name.StartsWith(name + "_level_sounds"))
                            soundscript =
                                new KeyValuePair<string, string>(internalDir + f.Name, externalDir + f.Name);
                        // presumably language files
                        else
                            langfiles.Add(new KeyValuePair<string, string>(internalDir + f.Name, externalDir + f.Name));
                    }
            }
            languages = langfiles;

            // ASW/Source2009 branch VScripts
            List<string> vscripts = new List<string>();

            foreach (Dictionary<string, string> entity in entityList)
            {
                foreach (KeyValuePair<string, string> kvp in entity)
                {
                    if (kvp.Key.ToLower() == "vscripts")
                    {
                        string[] scripts = kvp.Value.Split(' ');
                        foreach (string script in scripts)
                        {
                            vscripts.Add("scripts/vscripts/" + script);
                        }
                    }
                }
            }
            vscriptList = vscripts.Distinct().ToList();
        }

        public bool RenameNav { get; set; }

        public bool GenParticleManifest { get; set; }

        internal void findBspPakDependencies(string tempdir)
        {
            // Search the temp folder to find dependencies of files extracted from the pak file
            if (Directory.Exists(tempdir))
                foreach (var file in Directory.EnumerateFiles(tempdir, "*.vmt", SearchOption.AllDirectories))
                {
                    foreach (var material in findVmtMaterials(new FileInfo(file).FullName))
                        TextureList.Add(material);

                    foreach (var material in findVmtTextures(new FileInfo(file).FullName))
                        TextureList.Add(material);
                }
        }

        private List<string> findRadarDdsFiles(string fullpath)
        {
            // finds vmt files associated with radar overview files

            var DDSs = new List<string>();
            //var overviewFile = new KV.FileData(fullpath);
            var stream = new MemoryStream(System.IO.File.ReadAllBytes(fullpath));
            var overviewFile = KVSerializer.Create(KVSerializationFormat.KeyValues1Text).Deserialize(stream);

            // // Contains no blocks, return empty list
            // if (overviewFile.headnode.subBlocks.Count == 0)
            //     return DDSs;
            //
            // foreach (var subblock in overviewFile.headnode.subBlocks)
            // {
            //     var material = subblock.TryGetStringValue("material");
            //     // failed to get material, file contains no materials
            //     if (material == "")
            //         break;
            //
            //     string radarPath = $"resource/{vmtPathParser(material, false)}";
            //     // clean path so it never contains _radar
            //     if (radarPath.EndsWith("_radar"))
            //     {
            //         radarPath = radarPath.Replace("_radar", "");
            //     }
            //
            //     // add default radar
            //     DDSs.Add($"{radarPath}_radar.dds");
            //
            //     var verticalSections = subblock.GetFirstByName("verticalsections");
            //     if (verticalSections == null)
            //         break;
            //
            //     // add multi-level radars
            //     foreach (var section in verticalSections.subBlocks)
            //     {
            //         DDSs.Add($"{radarPath}_{section.name.Replace("\"", string.Empty)}_radar.dds");
            //     }
            // }

            return DDSs;
        }

        private string vmtPathParser(string vmtline, bool needsSplit = true)
        {
            if (needsSplit)
                vmtline = vmtline.Split(new char[] { ' ' }, 2)[1]; // removes the parameter name
            vmtline = vmtline.Split(new string[] { "//", "\\\\" }, StringSplitOptions.None)[0]; // removes endline parameter
            vmtline = vmtline.Trim(new char[] { ' ', '/', '\\' }); // removes leading slashes
            vmtline = vmtline.Replace('\\', '/'); // normalize slashes
            if (vmtline.StartsWith("materials/"))
                vmtline = vmtline.Remove(0, "materials/".Length); // removes materials/ if its the beginning of the string for consistency
            if (vmtline.EndsWith(".vmt") || vmtline.EndsWith(".vtf")) // removes extentions if present for consistency
                vmtline = vmtline.Substring(0, vmtline.Length - 4);
            return vmtline;
        }
        private List<string> findVmtMaterials(string fullpath)
        {
            // finds vmt files associated with vmt file

            var vmtList = new List<string>();
            foreach (var line in System.IO.File.ReadAllLines(fullpath))
            {
                var param = line.Replace("\"", " ").Replace("\t", " ").Trim();
                if (Keys.vmtMaterialKeyWords.Any(key => param.StartsWith(key + " ")))
                {
                    vmtList.Add("materials/" + vmtPathParser2(line) + ".vmt");
                }
            }
            return vmtList;
        }

        private List<string> findVmtTextures(string fullpath)
        {
            // finds vtfs files associated with vmt file

            var vtfList = new List<string>();
            foreach (var line in System.IO.File.ReadAllLines(fullpath))
            {
                var param = line.Replace("\"", " ").Replace("\t", " ").Trim();

                if (Keys.vmtTextureKeyWords.Any(key => param.ToLower().StartsWith(key + " ")))
                {
                    vtfList.Add("materials/" + vmtPathParser2(line) + ".vtf");
                    if (param.ToLower().StartsWith("$envmap" + " "))
                        vtfList.Add("materials/" + vmtPathParser2(line) + ".hdr.vtf");
                }
            }
            return vtfList;
        }

        public string vmtPathParser2(string vmtline)
        {
            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(vmtline + "\n")); // must add a newline to prevent parsing error
            var deserialized = KVSerializer.Create(KVSerializationFormat.KeyValues1Text).Deserialize(stream); // KVSerializationFormat ?
            var value = deserialized.Value.ToString();

            if (value == null)
            {
                System.Diagnostics.Trace.TraceError($"KVSerializer.Deserialize returned null: {vmtline}");
                return string.Empty;
            }

            value = value.Trim(new char[] { ' ', '/', '\\' }); // removes leading slashes
            value = value.Replace('\\', '/'); // normalize slashes
            value = Regex.Replace(value, "/+", "/"); // remove duplicate slashes

            if (value.StartsWith("materials/"))
                value = value.Remove(0, "materials/".Length); // removes materials/ if its the beginning of the string for consistency
            if (value.EndsWith(".vmt") || value.EndsWith(".vtf")) // removes extentions if present for consistency
                value = value.Substring(0, value.Length - 4);
            return value;
        }
    }
}
