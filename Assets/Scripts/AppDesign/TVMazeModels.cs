using System;

namespace AppDesign
{
    [Serializable]
    public class ShowSearchResult
    {
        public Show show;
    }

    [Serializable]
    public class Show
    {
        public int id;
        public string name;
        public string summary;
        public Image image;
    }

    [Serializable]
    public class Episode
    {
        public int id;
        public string name;
        public int season;
        public int number;
        public string summary;
    }

    [Serializable]
    public class Cast
    {
        public Person person;
        public Character character;
    }

    [Serializable]
    public class Person
    {
        public int id;
        public string name;
        public Image image;
    }

    [Serializable]
    public class Character
    {
        public int id;
        public string name;
        public Image image;
    }

    [Serializable]
    public class Image
    {
        public string medium;
        public string original;
    }
}

