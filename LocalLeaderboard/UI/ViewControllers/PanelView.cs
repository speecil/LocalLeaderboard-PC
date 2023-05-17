using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using IPA.Utilities;
using System.Linq;
using TMPro;
using UnityEngine;

namespace LocalLeaderboard.UI.ViewControllers
{
    [HotReload(RelativePathToLayout = @"../Views/PanelView.bsml")]
    [ViewDefinition("LocalLeaderboard.UI.Views.PanelView.bsml")]
    internal class PanelView : BSMLAutomaticViewController
    {
        private const float _skew = 0.18f;

        private ImageView _background;
        private ImageView _imgView;

        [UIComponent("container")]
        private Backgroundable _container;

        [UIComponent("LocalLeaderboard_logo")]
        private ImageView LocalLeaderboard_logo;

        [UIComponent("separator")]
        private ImageView _separator;

        internal static readonly FieldAccessor<ImageView, float>.Accessor ImageSkew = FieldAccessor<ImageView, float>.GetAccessor("_skew");
        internal static readonly FieldAccessor<ImageView, bool>.Accessor ImageGradient = FieldAccessor<ImageView, bool>.GetAccessor("_gradient");

        [UIComponent("totalScores")]
        public TextMeshProUGUI totalScores;

        [UIComponent("promptText")]
        public TextMeshProUGUI promptText;

        [UIComponent("lastPlayed")]
        public TextMeshProUGUI lastPlayed;

        [UIObject("prompt_loader")]
        public GameObject prompt_loader;


        LeaderboardView lb;

        [UIAction("#post-parse")]
        private void PostParse()
        {
            _container.background.material = Utilities.ImageResources.NoGlowMat;
            _imgView = _container.background as ImageView;
            _imgView.transform.position.Set(-5f, _imgView.transform.position.y, _imgView.transform.position.z);

            _imgView.color = new Color(0.156f, 0.69f, 0.46666f, 1);
            _imgView.color0 = Color.white;
            _imgView.color1 = new Color(1, 1, 1, 0);

            ImageSkew(ref _imgView) = _skew;
            ImageGradient(ref _imgView) = true;

            ImageSkew(ref LocalLeaderboard_logo) = _skew;
            LocalLeaderboard_logo.SetVerticesDirty();
            ImageSkew(ref _separator) = _skew;
            lb = Resources.FindObjectsOfTypeAll<LeaderboardView>().FirstOrDefault();
        }

        [UIAction("FunnyModalMoment")]
        private void FunnyModalMoment()
        {
            lb.showModal();
        }
    }
}
