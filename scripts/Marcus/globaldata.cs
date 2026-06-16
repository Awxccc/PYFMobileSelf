using UnityEngine;
using AYellowpaper.SerializedCollections;
using System.Collections.Generic;
using System.Linq;
[CreateAssetMenu(fileName = "Globaldata", menuName = "Globaldata")]
public class globaldataholder : ScriptableObject//use this for storing all global data like hats character classes etc
{
       public SerializedDictionary<int, character_class_data> character_classes;

       [SerializeField] private List<hat> easy_pool_hats;
        [SerializeField]private List<hat> hard_pool_hats;  

       public List<hat.hat_data> get_easyhats()
       {
              List<hat.hat_data> hats_copy = easy_pool_hats.Select(h => h.hat_info.copy()).ToList();
              return hats_copy;
       }
       public List<hat.hat_data> get_hardhats()
       {
              List<hat.hat_data> hats_copy = hard_pool_hats.Select(h => h.hat_info.copy()).ToList();
              return hats_copy;
       }
}
