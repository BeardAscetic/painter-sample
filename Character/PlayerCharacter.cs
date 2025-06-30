using System;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace BeardAscetic
{
    public partial class PlayerCharacter : CharacterBase, IPause
    {
        public PlayerRuntimeStatus RuntimeStatus;
        public PlayerHPBar PlayerHPBar;
        public Collider2D HitCollider;

        public AttackModule[] AttackModules;

        [Header("방향 인디케이터")]
        [SerializeField] private Transform directionIndicator;
        [SerializeField] private Transform attackIndicator;

        [Header("이동 설정")]
        public float moveSpeed = 5f;
        public bool canMove = true;

        public VirtualJoystick joystick;

        public Vector2 LastMoveDir => lastMoveDir;
        private Vector2 lastMoveDir = Vector2.up;

        public Vector2 LastAttackDir => lastAttackDir;
        private Vector2 lastAttackDir = Vector2.up;

        public bool IsMoveLeft => flipSubject.Value;
        private readonly BehaviorSubject<bool> flipSubject = new BehaviorSubject<bool>(false);
        private CompositeDisposable disposables = new CompositeDisposable();

        private Rigidbody2D rb;
        private Vector2 inputDir;
        
        [SerializeField] private float invincibleDuration = 2f;
        public bool CanTakeDamage => !RuntimeStatus.IsDead && !isInvincible;
        private bool isInvincible = false;
        
        private bool isPause = false;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            PlayerManager.Instance.RegisterPlayer(this);

            ColliderManager.RegisterPlayer(HitCollider, this);
            Init();
        }

        public void Init()
        {
            RuntimeStatus.InitStatus();
            RuntimeStatus.CurrentMoveSpeed.Subscribe(speed => moveSpeed = speed).AddTo(this);
            PlayerHPBar.Init();
            
            GamePlayManager.Instance.OnReviveCompleted
                .Subscribe(_ => OnRevive())
                .AddTo(this);

            RegisterPause();
        }

        public void OnGameStart()
        {
            foreach (var md in AttackModules)
            {
                md.StartLoop();
            }
        }

        // TODO : 외부 인풋 시스템
        private void Update()
        {
            if (!canMove) return;

            Vector2 joyDir = Vector2.zero;
            joyDir = joystick.InputDirection; 

            if (joyDir.sqrMagnitude > 0.01f)
            {
                inputDir = joyDir;
            }

            if (inputDir.sqrMagnitude > 0.01f)
            {
                lastMoveDir = inputDir.normalized;
                bool isLeft = lastMoveDir.x < 0f;
                flipSubject.OnNext(isLeft);
            }

            directionIndicator.up = lastMoveDir;

            UpdateAttackIndicator();
        }

        private void FixedUpdate()
        {
            if (!canMove || RuntimeStatus.IsDead || isPause) return;

            Vector2 move = (inputDir.sqrMagnitude > 1f)
                ? inputDir.normalized
                : inputDir;

            Vector2 newPos = rb.position + move * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(newPos);
        }

        public override void OnDamaged(int damage)
        {
            if (!TimeManager.IsRunning || isInvincible)
            {
                return;
            }

            DamageTextManager.TrySpawn(this.transform.position, damage);
            RuntimeStatus.OnDamage(damage);
            if (RuntimeStatus.IsDead)
            {
                OnDead();
            }
        }

        public void OnDead()
        {
            isInvincible = true;
            GamePlayManager.Instance.OnPlayerDead();
            foreach (var attackModule in AttackModules)
            {
                attackModule.StopLoop();
            }
            
            ColorManager.Instance.OnSplat(false, SplatType.Dot_1, this.transform.position, Quaternion.identity, 3f);
        }

        public void OnRevive()
        {
            StartInvincibilityAsync().Forget();
            RuntimeStatus.OnRevive();
            foreach (var attackModule in AttackModules)
            {
                attackModule.RestartLoop();
            }
        } 

        private async UniTaskVoid StartInvincibilityAsync()
        {
            isInvincible = true;
            await UniTask.Delay(TimeSpan.FromSeconds(invincibleDuration),
                cancellationToken: this.GetCancellationTokenOnDestroy());
            isInvincible = false;
        }

        private void OnDestroy()
        {
            disposables.Dispose();
            flipSubject.OnCompleted();
            flipSubject.Dispose();
        }

        public void RegisterFlipListener(Action<bool> onFlip)
        {
            flipSubject
                .DistinctUntilChanged()
                .Subscribe(onFlip)
                .AddTo(disposables);
        }

        public void RegisterPause()
        {
            TimeManager.Instance.Register(this);
        }

        public void PauseTime()
        {
            isPause = true;
        }

        public void ResumeTime()
        {
            isPause = false;
        }
    }
}
