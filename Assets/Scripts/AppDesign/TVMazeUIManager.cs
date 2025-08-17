using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

namespace AppDesign
{
    public class TVMazeUIManager : MonoBehaviour
    {
        private UIDocument _uiDocument;
        private TVMazeManager _tvMazeManager;

        public void SetUIDocument(UIDocument document)
        {
            _uiDocument = document;
        }

        public void SetTvMazeManager(TVMazeManager manager)
        {
            _tvMazeManager = manager;
        }
        
        public void DisplayShows(List<Show> shows)
        {
            var showList = _uiDocument.rootVisualElement.Q<ScrollView>("ShowList");
            showList.Clear();

            if (shows == null) return;

            foreach (var show in shows)
            {
                var showElement = new VisualElement();
                showElement.AddToClassList("show-element");

                var showImage = new VisualElement();
                showImage.AddToClassList("show-image");
                if (show.image != null && !string.IsNullOrEmpty(show.image.medium))
                {
                    StartCoroutine(LoadImage(show.image.medium, showImage));
                }

                showElement.Add(showImage);

                var showName = new Label(show.name);
                showName.AddToClassList("show-name");
                showElement.Add(showName);

                showElement.RegisterCallback<ClickEvent>(evt => ShowShowDetails(show));

                showList.Add(showElement);
            }
        }

        private void ShowShowDetails(Show show)
        {
            var showList = _uiDocument.rootVisualElement.Q<ScrollView>("ShowList");
            showList.Clear();

            var detailsContainer = new VisualElement();
            detailsContainer.AddToClassList("show-details-container");

            var showSummary = new Label();
            showSummary.text = StripHtml(show.summary);
            showSummary.AddToClassList("show-summary");
            detailsContainer.Add(showSummary);

            var episodesHeader = new Label("Episodes");
            episodesHeader.AddToClassList("details-header");
            detailsContainer.Add(episodesHeader);

            var episodesContainer = new ScrollView();
            episodesContainer.AddToClassList("episodes-container");
            detailsContainer.Add(episodesContainer);

            StartCoroutine(
                _tvMazeManager.GetEpisodes(show.id, episodes => DisplayEpisodes(episodes, episodesContainer)));

            var castHeader = new Label("Cast");
            castHeader.AddToClassList("details-header");
            detailsContainer.Add(castHeader);

            var castContainer = new ScrollView();
            castContainer.AddToClassList("cast-container");
            detailsContainer.Add(castContainer);

            StartCoroutine(_tvMazeManager.GetCast(show.id, cast => DisplayCast(cast, castContainer)));

            showList.Add(detailsContainer);
        }

        private void DisplayEpisodes(List<Episode> episodes, VisualElement container)
        {
            if (episodes == null) return;
            container.Clear();
            foreach (var episode in episodes)
            {
                var episodeElement = new VisualElement();
                episodeElement.AddToClassList("episode-element");
                var episodeName = new Label($"S{episode.season:00}E{episode.number:00}: {episode.name}");
                episodeName.AddToClassList("episode-name");
                episodeElement.Add(episodeName);
                episodeElement.RegisterCallback<ClickEvent>(evt => ShowEpisodeDetails(episode));
                container.Add(episodeElement);
            }
        }

        private void DisplayCast(List<Cast> cast, VisualElement container)
        {
            if (cast == null) return;
            container.Clear();
            foreach (var member in cast)
            {
                var castElement = new VisualElement();
                castElement.AddToClassList("cast-element");

                var personImage = new VisualElement();
                personImage.AddToClassList("person-image");
                if (member.person.image != null && !string.IsNullOrEmpty(member.person.image.medium))
                {
                    StartCoroutine(LoadImage(member.person.image.medium, personImage));
                }

                castElement.Add(personImage);

                var personName = new Label(member.person.name);
                personName.AddToClassList("person-name");
                castElement.Add(personName);

                var characterName = new Label($"as {member.character.name}");
                characterName.AddToClassList("character-name");
                castElement.Add(characterName);

                castElement.RegisterCallback<ClickEvent>(evt => ShowPersonDetails(member.person));

                container.Add(castElement);
            }
        }

        private void ShowEpisodeDetails(Episode episode)
        {
            var showList = _uiDocument.rootVisualElement.Q<ScrollView>("ShowList");
            showList.Clear();

            var detailsContainer = new VisualElement();
            detailsContainer.AddToClassList("show-details-container");

            var episodeName = new Label(episode.name);
            episodeName.AddToClassList("details-header");
            detailsContainer.Add(episodeName);

            var episodeSummary = new Label(StripHtml(episode.summary));
            episodeSummary.AddToClassList("show-summary");
            detailsContainer.Add(episodeSummary);

            showList.Add(detailsContainer);
        }

        private void ShowPersonDetails(Person person)
        {
            var showList = _uiDocument.rootVisualElement.Q<ScrollView>("ShowList");
            showList.Clear();

            var detailsContainer = new VisualElement();
            detailsContainer.AddToClassList("show-details-container");

            var personImage = new VisualElement();
            personImage.AddToClassList("person-image-large");
            if (person.image != null && !string.IsNullOrEmpty(person.image.original))
            {
                StartCoroutine(LoadImage(person.image.original, personImage));
            }

            detailsContainer.Add(personImage);

            var personName = new Label(person.name);
            personName.AddToClassList("details-header");
            detailsContainer.Add(personName);

            showList.Add(detailsContainer);
        }

        private string StripHtml(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            return Regex.Replace(input, "<.*?>", string.Empty);
        }

        private IEnumerator LoadImage(string url, VisualElement element)
        {
            using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url))
            {
                yield return webRequest.SendWebRequest();
                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    element.style.backgroundImage = new StyleBackground(DownloadHandlerTexture.GetContent(webRequest));
                }
            }
        }
    }
}