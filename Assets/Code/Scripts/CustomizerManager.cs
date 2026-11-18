using UnityEngine;

namespace Code.Scripts
{
    public class CustomizerManager : MonoBehaviour
    {
        [SerializeField] private GameObject player;

        private GameObject[] _hats;
        private int _activeHat;

        private GameObject[] _shirts;
        private int _activeShirt;

        private void Start()
        {
            _hats = GameObject.FindGameObjectsWithTag("Hat");
            _activeHat = 0;

            _shirts = GameObject.FindGameObjectsWithTag("Shirt");
            _activeShirt = 0;

            foreach (var hat in _hats)
            {
                hat.SetActive(false);
            }

            foreach (var shirt in _shirts)
            {
                shirt.SetActive(false);
            }

            _hats[_activeHat].SetActive(true);
            _shirts[_activeShirt].SetActive(true);
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

        public void SwapShirtLeft()
        {
            _shirts[_activeShirt].SetActive(false);

            _activeShirt -= 1;
            if (_activeShirt == -1)
            {
                _activeShirt = _shirts.Length - 1;
            }

            _shirts[_activeShirt].SetActive(true);
        }

        public void SwapShirtRight()
        {
            _shirts[_activeShirt].SetActive(false);

            _activeShirt += 1;
            if (_activeShirt == _shirts.Length)
            {
                _activeShirt = 0;
            }

            _shirts[_activeShirt].SetActive(true);
        }
    }
}