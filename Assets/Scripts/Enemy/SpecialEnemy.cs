using System;
using System.Collections;
using Pathfinding;
using UnityEngine;

namespace Enemy
{
    public class SpecialEnemy : MonoBehaviour
    {
        private const float Speed = 80f;
        private const float NextWaypoint = 3F;
        private const float SpriteScale = 0.2751575f;
        private const float ResetDelay = 0.5f;
        private const int StartingHealthPoint = 200;
        private const float TimeBetweenShots = 1f;
        private const string WeaponTag = "Weapon";
        private const string PlayerTag = "Player";
        [SerializeField] private Transform target;
        [SerializeField] private Transform spriteBoss;
        [SerializeField] private GameObject projectile;
        private Path _path;
        private Seeker _seeker;
        private Rigidbody2D _rigidbody2D;
        private Transform _target;
        private int _currentWaypoint;
        private int _healthPoint;
        private bool _isHit;
        private bool _reachedEndPath;
        private bool _hasShot;

        void Start()
        {
            if (GameManager.GameManagerInstance == null) return;
            _healthPoint = StartingHealthPoint;
            _seeker = GetComponent<Seeker>();
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _currentWaypoint = 0;
            _reachedEndPath = false;
            InvokeRepeating("UpdatePath", 0f, .5f);
        }

        void UpdatePath()
        {
            if (_seeker.IsDone())
                _seeker.StartPath(_rigidbody2D.position, target.position, OnPathComplete);
        }

        void OnPathComplete(Path path)
        {
            if (!path.error)
            {
                _path = path;
                _currentWaypoint = 0;
            }
        }

        void FixedUpdate()
        {
            if (_path == null) return;

            if (_currentWaypoint >= _path.vectorPath.Count)
            {
                _reachedEndPath = true;
                return;
            }

            _reachedEndPath = false;
            Vector2 direction = ((Vector2)_path.vectorPath[_currentWaypoint] - _rigidbody2D.position).normalized;
            Vector2 tempVelocity = direction * Speed * Time.deltaTime;
            _rigidbody2D.velocity = new Vector2(tempVelocity.x, tempVelocity.y);
            float distance = Vector2.Distance(_rigidbody2D.position, _path.vectorPath[_currentWaypoint]);
            if (distance < NextWaypoint)
                _currentWaypoint++;

            if (_rigidbody2D.velocity.x >= 0.1f)
                spriteBoss.localScale = new Vector3(SpriteScale, SpriteScale, SpriteScale);
            else if (_rigidbody2D.velocity.x <= -0.1f)
                spriteBoss.localScale = new Vector3(-SpriteScale, SpriteScale, SpriteScale);
        }

        private void Update()
        {
            if (_hasShot) return;
            StartCoroutine(DelayNextShot());
        }

        private IEnumerator DelayNextShot()
        {
            _hasShot = true;
            Instantiate(projectile, transform.position, Quaternion.identity);
            yield return new WaitForSeconds(TimeBetweenShots);
            _hasShot = false;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(PlayerTag))
                _target = other.transform;
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (!other.gameObject.CompareTag(WeaponTag) || GameManager.GameManagerInstance == null || _isHit) return;
            TakeDamage(GameManager.GameManagerInstance.GetPlayerDamage());
        }

        private void TakeDamage(int damage)
        {
            _isHit = true;
            transform.GetComponentInChildren<SpriteRenderer>().color = Color.red;
            if (_healthPoint - damage <= 0)
            {
                Destroy(GetComponent<Rigidbody2D>());
                Destroy(GetComponent<Collider2D>());
                Destroy(GetComponent<SpriteRenderer>());
                Destroy(gameObject);
                return;
            }

            _healthPoint -= damage;
            StartCoroutine(ResetHit());
        }

        private IEnumerator ResetHit()
        {
            yield return new WaitForSeconds(ResetDelay);
            _isHit = false;
        }

        private void OnCollisionExit2D(Collision2D other)
        {
            if (other.gameObject.CompareTag(WeaponTag))
                transform.GetComponentInChildren<SpriteRenderer>().color = Color.white;
        }
    }
}