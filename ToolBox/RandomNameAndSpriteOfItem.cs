using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GunRev
{
    [RequireComponent(typeof(PickupObject))]
class RandomiseSpriteAndNameOfItem : MonoBehaviour
{

    void Start()
    {
        _pickupObject = GetComponent<PickupObject>();

        _pickupObject.sprite.SetSprite(spriteIds[UnityEngine.Random.Range(0, spriteIds.Count)]);
    }

    public List<int> spriteIds = new List<int>();
    PickupObject _pickupObject;
}
}