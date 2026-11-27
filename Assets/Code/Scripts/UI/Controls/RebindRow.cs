using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RebindRow : MonoBehaviour
{
    [SerializeField] private TMP_Text labelText; 
    [SerializeField] private TMP_Text keyText;
    [SerializeField] private Button button;

    public Button Button => button;
    
    public void SetLabel(string label) => labelText.text = label;
    public void SetKey(string key) => keyText.text = key;
}