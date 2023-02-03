using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "AIScriptableObject")]
public class AISO : ScriptableObject {
    public List<Color> colors = new List<Color>(8);
    public List<Vector3> positions = new List<Vector3>(8);
}
