﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WindowsFormsApp15.model;

namespace WindowsFormsApp15.Data
{
    class DataSearch
    {
        public delegate bool Matches(string[] lines);

        private DataUtility du;
        public DataSearch()
        {
            du = new DataUtility();
        }

        /// <summary>
        /// Finds the Object that matches the given ID in the Entitie's storage file.
        /// </summary>
        /// <typeparam name="T">The type of Object that should be returned</typeparam>
        /// <param name="id">The ID of the Object</param>
        /// <exception cref="DuplicateDataException">More than one object with this ID was found.</exception>
        /// <returns>Object if ID can be found.
        /// Null if ID cannot be found.</returns>
        public T GetByID<T>(Guid id)
            where T : Storable, new()
        {
            string path = du.GetPath(typeof(T));
            Matches matches = (x) => x[0].Equals(id.ToString());
            List<string[]> matchingLines = GetAllMatchingLines(matches, path);
            if (matchingLines.Count == 0)
            {
                return null;
            }
            else if (matchingLines.Count != 1)
            {
                throw new DuplicateDataException("id occured multiple times in the file.");
            }
            return CreateNew<T>(matchingLines.First(), new T());
        }

        private T CreateNew<T>(string[] lines, Object o) where T : Storable
        {
            switch (o)
            {
                case University u:
                    return (T)Convert.ChangeType(new University(lines), typeof(T));
                case Course c:
                    return (T)Convert.ChangeType(new Course(lines), typeof(T));
                case Major m:
                    return (T)Convert.ChangeType(new Major(lines), typeof(T));
                case Rating r:
                    return (T)Convert.ChangeType(new Rating(lines), typeof(T));
                case Student s:
                    return (T)Convert.ChangeType(new Student(lines), typeof(T));
                case Lecturer l:
                    return (T)Convert.ChangeType(new Lecturer(lines), typeof(T));
                default:
                    return null;
            }
        }

        /// <summary>
        /// Return all Objects that are stored of that Entity.
        /// </summary>
        /// <typeparam name="T">Type of Objects that should be found.</typeparam>
        /// <returns></returns>
        public List<T> GetAll<T>()
            where T : Storable, new()
        {
            List<T> ts = new List<T>();
            string path = du.GetPath(typeof(T));
            Matches matches = (x) => true;
            List<string[]> matchingLines = GetAllMatchingLines(matches, path);
            foreach (string[] r in matchingLines)
            {
                T t = CreateNew<T>(r, new T());
                ts.Add(t);
            }
            return ts;
        }

        /// <summary>
        /// Return all Objects that are stored of that Entity.
        /// </summary>
        /// <typeparam name="T">Type of Objects that should be found.</typeparam>
        /// <returns></returns>
        public List<T> GetAllMatching<T>(Matches match)
            where T : Storable, new()
        {
            List<T> ts = new List<T>();
            string path = du.GetPath(typeof(T));
            List<string[]> matchingLines = GetAllMatchingLines(match, path);
            foreach (string[] r in matchingLines)
            {
                T t = CreateNew<T>(r, new T());
                ts.Add(t);
            }
            return ts;
        }

        /// <summary>
        /// Returns wether a object with the specified conditions exists in the files.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="condition"></param>
        /// <returns></returns>
        public bool ObjectExists<T>(Matches condition) where T : Storable
        {
            string path = du.GetPath(typeof(T));
            List<string[]> matchingLines = GetAllMatchingLines(condition, path);
            return matchingLines.Count > 0;
        }

        /// <summary>
        /// Finds all universities which offer a certain major.
        /// </summary>
        /// <param name="major"></param>
        /// <returns></returns>
        public List<University> GetUniversitiesWithMajor(Major major)
        {
            List<University> unis = new List<University>();
            Matches matches = (x) => x[1].Equals(major.Name);
            List<string[]> matchingLines = GetAllMatchingLines(matches, du.MajorPath);
            List<Guid> foundUnis = new List<Guid>();
            foreach (string[] r in matchingLines)
            {
                try
                {
                    Guid g = Guid.Parse(r[2]);
                    foundUnis.Add(g);
                }
                catch (Exception)
                {
                    continue;
                }
            }
            foundUnis = foundUnis.Distinct().ToList();
            foreach (Guid uniID in foundUnis)
            {
                try
                {
                    University u = GetByID<University>(uniID);
                    if (u != null)
                    {
                        unis.Add(u);
                    }
                }
                catch (DuplicateDataException)
                {
                    continue;
                }
            }
            return unis;
        }

        /// <summary>
        /// Gets all the lecturers who teach a certain major.
        /// </summary>
        /// <param name="major"></param>
        /// <returns></returns>
        public List<Lecturer> GetLecturersFromMajor(Major major)
        {
            List<Lecturer> lecturers = new List<Lecturer>();
            Matches matches = (x) => x[3].Equals(major.ID.ToString());
            List<string[]> matchingLines = GetAllMatchingLines(matches, du.LecturerPath);
            foreach (string[] r in matchingLines)
            {
                Lecturer lecturer = new Lecturer(r);
                lecturers.Add(lecturer);
            }
            return lecturers;
        }

        /// <summary>
        /// Gets all the courses that match the keyword.
        /// Also includes courses where the keyword appears in the middle of the name (not only the ones
        /// where it occurs at the beginning).
        /// Is not case sensitive.
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public List<Course> GetCoursesByKeyword(string keyword)
        {
            List<Course> courses = new List<Course>();
            //putting both strings as ToUpper() will make the search case insensitive
            Matches matches = (x) => x[1].ToUpper().Contains(keyword.ToUpper());
            List<string[]> matchingLines = GetAllMatchingLines(matches, du.CoursePath);
            foreach (string[] r in matchingLines)
            {
                Course course = new Course(r);
                courses.Add(course);
            }
            return courses;
        }

        /// <summary>
        /// Gets all the courses in this major
        /// </summary>
        /// <param name="major"></param>
        /// <returns></returns>
        public List<Course> GetCoursesByMajor(Major major)
        {
            List<Course> courses = new List<Course>();
            Matches matches = (x) => x[5].Equals(major.ID.ToString());
            List<string[]> matchingLines = GetAllMatchingLines(matches, du.CoursePath);
            foreach (string[] r in matchingLines)
            {
                Course course = new Course(r);
                courses.Add(course);
            }
            return courses;
        }

        /// <summary>
        /// Gets all Courses that are held by this lecturer.
        /// </summary>
        /// <param name="lecturer"></param>
        /// <returns></returns>
        public List<Course> GetCoursesByLecturer(Lecturer lecturer)
        {
            List<Course> courses = new List<Course>();
            Matches matches = (x) => x[3].Equals(lecturer.ID.ToString());
            List<string[]> matchingLines = GetAllMatchingLines(matches, du.CoursePath);
            foreach (string[] r in matchingLines)
            {
                Course course = new Course(r);
                courses.Add(course);
            }
            return courses;
        }

        /// <summary>
        /// Gets all the ratings that were given for a certain course.
        /// </summary>
        /// <param name="course"></param>
        /// <returns></returns>
        public List<Rating> GetRatingsByCourse(Course course)
        {
            List<Rating> ratings = new List<Rating>();
            Matches matches = (x) => x[2].Equals(course.ID.ToString());
            List<string[]> matchingLines = GetAllMatchingLines(matches, du.RatingPath);
            foreach (string[] r in matchingLines)
            {
                Rating rating = new Rating(r);
                ratings.Add(rating);
            }
            return ratings;
        }

        /// <summary>
        /// Gets all the majors offered by a university by iterating thorgh MajorStorage file.
        /// </summary>
        /// <param name="university"></param>
        /// <returns>List of majors of the university</returns>
        public List<Major> GetMajorsOfUniversity(University university)
        {
            List<Major> majors = new List<Major>();
            Matches matches = (x) => x[2].Equals(university.ID.ToString());
            List<string[]> matchingLines = GetAllMatchingLines(matches, du.MajorPath);
            foreach (string[] r in matchingLines)
            {
                Major major = new Major(r);
                majors.Add(major);
            }
            return majors;
        }

        /// <summary>
        /// Calculates the average overallRating of the Course by iterating over all Ratings for the course.
        /// </summary>
        /// <param name="course"></param>
        /// <returns></returns>
        public double AverageRatingForCourse(Course course)
        {
            double sum = 0;
            Matches matches = (x) => x[2].Equals(course.ID.ToString());
            List<string[]> matchingLines = GetAllMatchingLines(matches, du.RatingPath);
            foreach (string[] r in matchingLines)
            {
                sum += Int32.Parse(r[4]);
            }
            return sum / matchingLines.Count;
        }

        /// <summary>
        /// Calculates the average overallRating of the courses in this major 
        /// by iterating over all courses and getting their average overallRating.
        /// </summary>
        /// <param name="major"></param>
        /// <returns></returns>
        public double AverageRatingForMajor(Major major)
        {
            double sum = 0;
            List<Course> courses = GetCoursesByMajor(major);
            foreach (Course course in courses)
            {
                sum += AverageRatingForCourse(course);
            }
            return sum / courses.Count;
        }

        /// <summary>
        /// Calculates the average overallRating of the majors at this university
        /// by iterating over all majors and getting their average overallRating.
        /// </summary>
        /// <param name="university"></param>
        /// <returns></returns>
        public double AverageRatingForUniversity(University university)
        {
            double sum = 0;
            List<Major> majors = GetMajorsOfUniversity(university);
            foreach (Major major in majors)
            {
                sum += AverageRatingForMajor(major);
            }
            return sum / majors.Count;
        }

        /// <summary>
        /// Calculates the average overallRating of the courses by this lecturer 
        /// by iterating over all courses and getting their average overallRating.
        /// </summary>
        /// <param name="lecturer"></param>
        /// <returns></returns>
        public double AverageRatingForLecturer(Lecturer lecturer)
        {
            double sum = 0;
            List<Course> courses = GetCoursesByLecturer(lecturer);
            foreach (Course course in courses)
            {
                sum += AverageRatingForCourse(course);
            }
            return sum / courses.Count;
        }

        /// <summary>
        /// Returns the average overallRating of all courses in this major, the amount
        /// of courses in this major and the amount of lecturers in this major;
        /// Should only be used by UniSearchWindow!!!
        /// </summary>
        /// <param name="major"></param>
        /// <returns></returns>
        public Tuple<double, int, int> AverageRatingAmountCoursesAmountLecturersForMajor(Major major)
        {
            double sumCourses = 0;
            List<Course> courses = GetCoursesByMajor(major);
            foreach (Course course in courses)
            {
                sumCourses += AverageRatingForCourse(course);
            }

            Matches matches = (x) => x[3].Equals(major.ID.ToString());
            int foundLecturers = GetNumberOfMatchingLines(matches, du.LecturerPath);

            return new Tuple<double, int, int>(sumCourses / courses.Count, courses.Count, foundLecturers);
        }

        /// <summary>
        /// Returns the average overallRating of this course and the amount of ratings the course has.
        /// Should only be used by ProfessorSearchResultWindow and CoursewSearchResultWindow!!!
        /// </summary>
        /// <param name="course"></param>
        /// <returns></returns>
        public Tuple<double, int> AverageRatingAmountRatingsForCourse(Course course)
        {
            double sum = 0;
            Matches matches = (x) => x[2].Equals(course.ID.ToString());
            List<string[]> matchingLines = GetAllMatchingLines(matches, du.RatingPath);
            foreach (string[] r in matchingLines)
            {
                sum += Int32.Parse(r[4]);
            }
            return new Tuple<double, int>(sum / matchingLines.Count, matchingLines.Count);
        }

        /// <summary>
        /// Calculates the average overallRating of all courses at the university, as well as the
        /// number of professors teaching there and the amount of courses and majors offered.
        /// Should only be used by MajorSearchResultWindow!!!
        /// </summary>
        /// <param name="university"></param>
        /// <returns></returns>
        public Tuple<double, int, int, int> AverageRatingAmountCoursesMajorsProfessors(University university)
        {
            Matches matchesMajor = (x) => x[2].Equals(university.ID.ToString());
            List<string[]> matchingMajors = GetAllMatchingLines(matchesMajor, du.MajorPath);

            Matches matchesProfessor = (x) => x[2].Equals(university.ID.ToString());
            int amountProfessors = GetNumberOfMatchingLines(matchesProfessor, du.LecturerPath);
            double ratingSum = 0;
            int countRatings = 0;
            int countCourses = 0;
            foreach (string[] major in matchingMajors)
            {
                Matches matchesCourse = (x) => x[2].Equals(major);
                List<string[]> matchingCourses = GetAllMatchingLines(matchesCourse, du.CoursePath);
                countCourses += matchingCourses.Count;
                foreach (string[] course in matchingCourses)
                {
                    Matches matchesRating = (x) => x[2].Equals(course[0]);
                    List<string[]> matchingRatings = GetAllMatchingLines(matchesRating, du.RatingPath);
                    countRatings += matchingRatings.Count;
                    foreach (string[] rating in matchingRatings)
                    {
                        ratingSum += Int32.Parse(rating[4]);
                    }
                }
            }
            return new Tuple<double, int, int, int>(ratingSum / countRatings, countCourses,
                matchingMajors.Count, amountProfessors);
        }

        private List<string[]> GetAllMatchingLines(Matches match, string path)
        {
            List<string[]> results = new List<string[]>();
            using (StreamReader sr = File.OpenText(path))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] splitLine = line.Split(';');
                    if (match(splitLine))
                        results.Add(splitLine);

                }
            }
            return results;
        }

        private int GetNumberOfMatchingLines(Matches match, string path)
        {
            int number = 0;
            using (StreamReader sr = File.OpenText(path))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] splitLine = line.Split(';');
                    if (match(splitLine))
                        number++;

                }
            }
            return number;
        }

    }
}
