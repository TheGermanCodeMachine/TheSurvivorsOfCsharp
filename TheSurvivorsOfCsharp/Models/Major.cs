﻿using System;
using WindowsFormsApp15.Data;

namespace WindowsFormsApp15.model
{
    public class Major : Storable
    {
        private University university;
        private int majorID;
        private string majorName;

        /// <summary>
        /// Constructor for the class Major without giving a majorID.
        /// ID will be generated automatically.
        /// Should be used when creating a truly new object that is not yet stored.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="university"></param>
        public Major(string name, University university)
        {
            DataSearch ds = new DataSearch();
            Init(name, university);
        }

        /// <summary>
        /// Constructs a new Entity of Major from a line from the datafiles.
        /// Should only be used when creating objects from files!
        /// </summary>
        /// <param name="line"></param>
        public Major(string[] line)
        {
            DataSearch ds = new DataSearch();
            University u;
            try
            {
                u = ds.GetByID<University>(Guid.Parse(line[2]));
            }
            catch (DuplicateDataException)
            {
                u = new University();
            }
            Init(line[1], u); ;
        }

        public string Name { get => majorName; }
        public University University { get => university; }
        public override int ID { get; set; }
        private void Init(string name, University university)
        {
            this.majorName = name ?? throw new ArgumentNullException("name cannot be null!");
            this.university = university ?? throw new ArgumentNullException("university cannot be null!");
        }

        /// <summary>
        /// Return a string with the objects properties in the format:
        /// "majorID;majorName;universityID".
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string info = ID.ToString() + ";" +
                Name + ";" +
                University.ID.ToString() + "\n";
            return info;
        }

        public Major() : base() { }
    }
}
