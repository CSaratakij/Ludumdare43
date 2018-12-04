using UnityEngine;

namespace Ludumdare43
{
    public class SortingOrderUpdater : MonoBehaviour
    {
        int cacheSortingOrder;
        int oldSortingOrder;

        SpriteRenderer spriteRenderer;


        void Awake()
        {
            Initialize();
        }

        void LateUpdate()
        {
            UpdateSortingOrder();
        }

        void Initialize()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        void UpdateSortingOrder()
        {
            oldSortingOrder = cacheSortingOrder;
            cacheSortingOrder = (int)(transform.position.y * -100.0f);

            if (cacheSortingOrder == oldSortingOrder)
                return;

            spriteRenderer.sortingOrder = cacheSortingOrder;
        }
    }
}
