using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using PkMn.Model;
using PkMn.Model.Enums;

namespace PkMn.Instance
{
    public class Trainer
    {
        public string Name;
        public string MonNamePrefix;
        public Monster[] Party;
        public bool IsPlayer;

        public Trainer()
        {
        }

        public Trainer(XmlNode node)
        {
            Name = node.Attributes["name"].Value;
            IsPlayer = node.Attributes.Contains("is-player") ? bool.Parse(node.Attributes["is-player"].Value) : false;
            MonNamePrefix = node.Attributes.Contains("mon-name-prefix") ? node.Attributes["mon-name-prefix"].Value.Trim() + " " : "";

            XmlNode[] monsters = node.ChildNodes.Cast<XmlNode>().Where(n => n.Name == "monster").ToArray();

            List<string> remainingMonsters = Species.Spp.Select(p => p.Key).ToList();
            foreach(string sp in monsters.Where( m => m.Attributes.Contains("species") && m.Attributes["species"].Value != "Random").Select(m => m.Attributes["species"].Value))
            {
                if (remainingMonsters.Contains(sp))
                    remainingMonsters.Remove(sp);
            }

            Party = new Monster[monsters.Length];

            for (int i = 0; i < monsters.Length; i++)
            {
                if (monsters[i].Attributes["species"].Value == "Random")
                {
                    int level = 5;
                    if (monsters[i].Attributes.Contains("level"))
                        level = int.Parse(monsters[i].Attributes["level"].Value);

                    if(remainingMonsters.Count <= 0)
                        remainingMonsters = Species.Spp.Select(p => p.Key).ToList();

                    Party[i] = new Monster(remainingMonsters[Rng.Next(1, remainingMonsters.Count)], level, IsPlayer ? Generator.SimulatePlayer : Generator.Trainer);

                    if (remainingMonsters.Contains(Party[i].Species.Name))
                        remainingMonsters.Remove(Party[i].Species.Name);
                }
                else
                    Party[i] = new Monster(monsters[i], IsPlayer ? Generator.SimulatePlayer : Generator.Trainer);
            }
        }

        public XmlNode ToXml(XmlDocument doc)
        {
            XmlNode trainer = doc.CreateElement("party");

            trainer.AppendAttribute("name", Name);

            if (IsPlayer)
                trainer.AppendAttribute("is-player", IsPlayer.ToString().ToLower());

            if (!string.IsNullOrWhiteSpace(MonNamePrefix))
                trainer.AppendAttribute("mon-name-prefix", MonNamePrefix.Trim());

            foreach (Monster monster in Party)
                if (monster != null)
                    trainer.AppendChild(monster.ToXml(doc));

            return trainer;
        }

        public static void SaveToFile(string path, params Trainer[] trainers)
        {
            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.CreateXmlDeclaration("1.0", null, null));
            XmlNode parties = doc.CreateElement("parties");
            foreach (Trainer trainer in trainers)
                parties.AppendChild(trainer.ToXml(doc));
            doc.AppendChild(parties);

            using (XmlTextWriter writer = new XmlTextWriter(path, Encoding.UTF8))
            {
                writer.Formatting = Formatting.Indented;
                writer.IndentChar = '\t';
                writer.Indentation = 1;
                doc.Save(writer);
            }
           
        }

        public static Trainer[] ReadFromFile(string path)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            List<Trainer> parties = new List<Trainer>();

            foreach (XmlNode party in doc.GetElementsByTagName("party"))
            {
                parties.Add(new Trainer(party));
            }
            
            return parties.ToArray();
        }

        public static void ReadFromFile(string path, out Trainer player, out Trainer opponent)
        {
            Trainer[] trainers = ReadFromFile(path);
            if (trainers.Length != 2)
                throw new Exception(string.Format("File {0} contains {1} trainer(s). Expected exactly two.", path, trainers.Length));

            player = trainers.Where(t => t.IsPlayer).FirstOrDefault();
            opponent = trainers.Where(t => !t.IsPlayer).FirstOrDefault();

            if (player == null)
                throw new Exception("No party with is-player = 'true' found.");
            if(opponent == null)
                throw new Exception("No party for computer opponent found.");

        }
    }
}
