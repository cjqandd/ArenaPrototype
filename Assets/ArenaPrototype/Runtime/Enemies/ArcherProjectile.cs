using ArenaPrototype.Combat;
using ArenaPrototype.Art;
using UnityEngine;

namespace ArenaPrototype.Enemies
{
    public sealed class ArcherProjectile : MonoBehaviour
    {
        private readonly RaycastHit[] hitResults = new RaycastHit[8];

        private GameObject owner;
        private Vector3 direction;
        private float speed;
        private float remainingLifetime;
        private float hitRadius;
        private float healthDamage;
        private float knockbackSpeed;

        public static void Spawn(
            Vector3 position,
            Vector3 newDirection,
            Material material,
            EquipmentVisualProfile visualProfile,
            GameObject newOwner,
            float newSpeed,
            float lifetime,
            float newHitRadius,
            float newHealthDamage,
            float newKnockbackSpeed)
        {
            GameObject arrow = new GameObject("ArcherProjectile");
            arrow.name = "ArcherProjectile";
            arrow.transform.SetPositionAndRotation(
                position,
                Quaternion.LookRotation(newDirection, Vector3.up));

            GameObject productionModel = ArenaVisualFactory.InstantiateProjectileModel(
                visualProfile,
                arrow.transform);
            if (productionModel == null)
            {
                GameObject grayboxModel = GameObject.CreatePrimitive(PrimitiveType.Cube);
                grayboxModel.name = "Visual_Graybox";
                grayboxModel.transform.SetParent(arrow.transform, false);
                grayboxModel.transform.localScale = new Vector3(0.08f, 0.08f, 0.75f);
                Destroy(grayboxModel.GetComponent<Collider>());
                grayboxModel.GetComponent<Renderer>().sharedMaterial = material;
            }

            ArcherProjectile projectile = arrow.AddComponent<ArcherProjectile>();
            projectile.owner = newOwner;
            projectile.direction = newDirection.normalized;
            projectile.speed = newSpeed;
            projectile.remainingLifetime = lifetime;
            projectile.hitRadius = newHitRadius;
            projectile.healthDamage = newHealthDamage;
            projectile.knockbackSpeed = newKnockbackSpeed;
        }

        public static void DestroyAllActive()
        {
            ArcherProjectile[] projectiles = FindObjectsByType<ArcherProjectile>(
                FindObjectsSortMode.None);
            foreach (ArcherProjectile projectile in projectiles)
            {
                if (projectile != null)
                {
                    Destroy(projectile.gameObject);
                }
            }
        }

        private void Update()
        {
            if (Time.timeScale <= 0f)
            {
                return;
            }

            float travelDistance = speed * Time.deltaTime;
            if (TryHit(travelDistance))
            {
                Destroy(gameObject);
                return;
            }

            transform.position += direction * travelDistance;
            remainingLifetime -= Time.deltaTime;
            if (remainingLifetime <= 0f)
            {
                Destroy(gameObject);
            }
        }

        private bool TryHit(float travelDistance)
        {
            int hitCount = Physics.SphereCastNonAlloc(
                transform.position,
                hitRadius,
                direction,
                hitResults,
                travelDistance,
                ~0,
                QueryTriggerInteraction.Ignore);

            RaycastHit closestHit = default;
            float closestDistance = float.PositiveInfinity;
            bool foundHit = false;

            for (int i = 0; i < hitCount; i++)
            {
                Collider hitCollider = hitResults[i].collider;
                if (hitCollider == null
                    || (owner != null && hitCollider.transform.IsChildOf(owner.transform)))
                {
                    continue;
                }

                if (hitResults[i].distance < closestDistance)
                {
                    closestDistance = hitResults[i].distance;
                    closestHit = hitResults[i];
                    foundHit = true;
                }
            }

            if (!foundHit)
            {
                return false;
            }

            PlayerHealth playerHealth = closestHit.collider.GetComponentInParent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.ReceiveHit(new CombatHit(
                    owner,
                    healthDamage,
                    0f,
                    direction,
                    knockbackSpeed,
                    CombatAttackKind.Projectile));
            }

            return true;
        }
    }
}
