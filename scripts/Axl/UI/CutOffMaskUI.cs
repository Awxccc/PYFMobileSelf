using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
/** 
@brief Controls the mask used for the fade in and out
*/
public class CutOffMaskUI : Image
{
    public override Material materialForRendering
    {
        get
        {
            Material material = new Material(base.materialForRendering);
            material.SetInt("_StencilComp", (int)CompareFunction.NotEqual);
            return material;
        }
    }
}
