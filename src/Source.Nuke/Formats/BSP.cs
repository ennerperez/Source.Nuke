using System;
using System.Collections.Generic;
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
        static class Keys
        {
            public static IEnumerable<string> vmfSoundKeys = new []
            {
                "breaksound",
                "closesound",
                "gassound",
                "gustsound",
                "locked_sound",
                "message",
                "messagesound",
                "movementsound",
                "movepingsound",
                "movesound",
                "noise1",
                "noise2",
                "puntsound",
                "reversalsoundlarge",
                "reversalsoundmedium",
                "reversalsoundsmall",
                "slidesoundback",
                "slidesoundfwd",
                "soundcloseoverride",
                "soundlockedoverride",
                "soundmoveoverride",
                "soundopenoverride",
                "soundunlockedoverride",
                "startclosesound",
                "startsound",
                "stopsound",
                "unlocked_sound",
                "point_allies_capsound",
                "point_axis_capsound",
                "point_resetsound",
                "fireendsound",
                "firestartsound",
                "flysound",
                "incomingsound",
                "launchsound",
                "pulsefiresound",
                "rotatesound",
                "rotatestartsound",
                "rotatestopsound",
                "team_capsound_0",
                "team_capsound_2",
                "team_capsound_3",
                "disablesound",
                "m_SoundName",
                "commentaryfile",
                "commentaryfile_nohdr",
            };
            public static IEnumerable<string> vmfModelKeys = new []
            {
                "model",
                "shootmodel",
                "point_allies_model",
                "point_axis_model",
                "point_reset_model",
                "missilemodel",
                "team_model_0",
                "team_model_2",
                "team_model_3",
                "flag_model",
                "plymodel",
                "powerup_model",
                "swapmodel",
            };
            public static IEnumerable<string> vmfMaterialKeys => new[]
            {
                "material",
                "texture",
                "ropematerial",
                "overlaymaterial",
                "point_hud_icon_neutral",
                "point_hud_icon_axis",
                "point_hud_icon_allies",
                "point_hud_icon_timercap",
                "point_hud_icon_bombed",
                "spritename",
                "team_icon_0",
                "team_icon_2",
                "team_icon_3",
                "team_overlay_0",
                "team_overlay_2",
                "team_overlay_3",
                "team_base_icon_2",
                "team_base_icon_3",
                "overlaymaterial",
                "particletrailmaterial",
                "overlayname1",
                "overlayname2",
                "overlayname3",
                "overlayname4",
                "overlayname5",
                "overlayname6",
                "overlayname7",
                "overlayname8",
                "overlayname9",
                "overlayname10",
                "font",
            };
            public static IEnumerable<string> vmtTextureKeyWords = new []
            {
                "$ambientocclusiontexture",
                "$basetexture",
                "$basetexture2",
                "$basetexture3",
                "$basetexture4",
                "$decaltexture",
                "$blendmodulatetexture",
                "$bumpmap",
                "$bumpmap2",
                "$bumpmask",
                "$cloudalphatexture",
                "$detail",
                "$detail2",
                "$tintmasktexture",
                "$masks",
                "$masks1",
                "$masks2",
                "$dudvmap",
                "$envmap",
                "$envmapmask",
                "$envmapmask2",
                "$flowmap",
                "$flowbounds",
                "$flow_noise_texture",
                "$emissiveblendtexture",
                "$emissiveblendbasetexture",
                "$emissiveblendflowtexture",
                "$fleshbordertexture1d",
                "$fleshcubetexture",
                "$fleshinteriornoisetexture",
                "$fleshinteriortexture",
                "$fleshnormaltexture",
                "$fleshsubsurfacetexture",
                "$hdrbasetexture",
                "$hdrcompressedtexture",
                "$lightwarptexture",
                "$material",
                "$normalmap",
                "$fresnelrangestexture",
                "$phongexponenttexture",
                "$phongwarptexture",
                "$reflecttexture",
                "$refracttexture",
                "$refracttinttexture",
                "$selfillummask",
                "$selfillumtexture",
                "$texture",
                "$texture2",
                "$worldspacetint",
                "$worldspacetype",
                "$emissiveBlendTexture",
                "$emissiveBlendBaseTexture",
                "$emissiveBlendFlowTexture",
                "%tooltexture",
                "srgb?$basetexture",
                "$iris",
                "$ambientoccltexture",
                "$mraotexture",
                "$emissiontexture",
                "$portalmasktexture",
                "$portalcolortexture",
            };
            public static IEnumerable<string> vmtMaterialKeyWords = new []
            {
                "$bottommaterial",
                "$crackmaterial",
                "$fallbackmaterial",
                "$underwateroverlay",
                "include",
                "material",
            };

        }

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

        public FileInfo file { get; private set; }
        public string gameFolder { get; private set; }
        private bool isL4D2 = false;
        private int bspVersion;

        public BSP(FileInfo file, string gameFolder)
        {
            this.gameFolder = gameFolder;
            this.file = file;

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

        public void buildEntityList()
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

        public void buildTextureList()
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

        public void buildEntTextureList()
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

                if(ent["classname"].Contains("skybox_swapper") && ent.ContainsKey("SkyboxName") )
                {
                    if(ent.ContainsKey("targetname"))
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
			            var directory = $"{gameFolder}/materials/vgui/{ent["directory"]}";
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

                    switch (command) {
                        case "SetCountdownImage":
                            materials.Add($"vgui/{parameter}");
                            break;
                        case "Command":
                            // format of Command is <command> <parameter>
                            if(!parameter.Contains(' '))
                            {
                                continue;
                            }
                            (command, parameter) = parameter.Split(' ') switch { var param => (param[0], param[1])};
                            if (command == "r_screenoverlay")
                                materials.Add(parameter);
                            break;
                        case "AddOutput":
                            if(!parameter.Contains(' '))
                            {
                                continue;
                            }
                            string k, v;
                            (k,v) = parameter.Split(' ') switch { var a => (a[0], a[1])};

                            // support packing mats when using addoutput to change skybox_swappers
                            if(skybox_swappers.Contains(target.ToLower()) && k.ToLower() == "skyboxname")
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

        public void buildModelList()
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

        public void buildEntModelList()
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

        public void buildEntSoundList()
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
                        if(!parameter.Contains(' '))
                            continue;

                        (command, parameter) = parameter.Split(' ') switch { var param => (param[0], param[1])};

                        if (command == "play" || command == "playgamesound" )
                            EntSoundList.Add($"sound/{parameter}");
                    }
                }
            }
        }
        // color correction, etc.
        public void buildMiscList()
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

        public void buildParticleList()
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
                //CompilePalLogger.LogCompileError($"Failed to decode IO, ignoring: {property}\n", new Error($"Failed to decode IO, ignoring: {property}\n", ErrorSeverity.Warning));
                throw new OperationCanceledException($"Failed to decode IO, ignoring: {property}\n");
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
                        //CompilePalLogger.LogCompileError($"Failed to decode AddOutput, format may be incorrect: {property}\n", new Error($"Failed to decode AddOutput, format may be incorrect: {property}\n", ErrorSeverity.Warning));
                        throw new OperationCanceledException($"Failed to decode AddOutput, format may be incorrect: {property}\n");
                        return null;
                    }

                    targetInput = splitIo[1];
                    parameter = splitIo[2];
                }
            }

            return new Tuple<string, string, string>(io[0], targetInput, parameter);
        }

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

        private static List<string> findVmtMaterials(string fullpath)
        {
            // finds vmt files associated with vmt file

            var vmtList = new List<string>();
            foreach (var line in File.ReadAllLines(fullpath))
            {
                var param = line.Replace("\"", " ").Replace("\t", " ").Trim();
                if (Keys.vmtMaterialKeyWords.Any(key => param.StartsWith(key + " ")))
                {
                    vmtList.Add("materials/" + vmtPathParser2(line) + ".vmt");
                }
            }
            return vmtList;
        }

        private static List<string> findVmtTextures(string fullpath)
        {
            // finds vtfs files associated with vmt file

            var vtfList = new List<string>();
            foreach (var line in File.ReadAllLines(fullpath))
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

        public static string vmtPathParser2(string vmtline)
        {
            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(vmtline + "\n")); // must add a newline to prevent parsing error
            var deserialized = KVSerializer.Create(KVSerializationFormat.KeyValues1Text).Deserialize(stream); // KVSerializationFormat ?
            var value = deserialized.Value.ToString();

            if(value == null)
            {
                // CompilePalLogger.LogCompileError($"KVSerializer.Deserialize returned null: {vmtline}",
                //     new Error($"KVSerializer.Deserialize returned null: {vmtline}", ErrorSeverity.Error));
                // return "";
                throw new OperationCanceledException($"KVSerializer.Deserialize returned null: {vmtline}");
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
