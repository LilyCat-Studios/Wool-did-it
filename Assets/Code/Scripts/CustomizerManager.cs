using UnityEngine;

namespace Code.Scripts
{
    public class CustomizerManager : MonoBehaviour
    {
        [SerializeField] private GameObject player;

        private GameObject[] _hats;
        private int _activeHat;

        private void Start()
        {
            _hats = GameObject.FindGameObjectsWithTag("Hat");
            _activeHat = 0;

            foreach (var hat in _hats)
            {
                hat.SetActive(false);
            }
        }

        public void SwapHatLeft()
        {
            _hats[_activeHat].SetActive(false);

            _activeHat -= 1;
            if (_activeHat == -1)
            {
                _activeHat = _hats.Length - 1;
            }

            _hats[_activeHat].SetActive(true);
        }

        public void SwapHatRight()
        {
            _hats[_activeHat].SetActive(false);

            _activeHat += 1;
            if (_activeHat == _hats.Length)
            {
                _activeHat = 0;
            }

            _hats[_activeHat].SetActive(true);
        }
    }
}