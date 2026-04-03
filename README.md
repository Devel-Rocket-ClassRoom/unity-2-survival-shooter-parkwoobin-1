# Survival Shooter 모작 과제

## 개요

이 프로젝트는 Unity 기반 탑다운 3D 슈터 게임 모작입니다.
플레이어 이동, 조준/사격, 좀비 AI, 웨이브 스폰, 점수/UI, 게임오버, 일시정지 메뉴 및 오디오 볼륨 제어를 포함합니다.

## 아키텍처 요약

- 엔티티 계층: [LivingEntity.cs](LivingEntity.cs) 를 기반으로 플레이어/좀비가 체력 및 사망 로직을 공유합니다.
- 플레이어 계층: [PlayerInput.cs](PlayerInput.cs), [PlayerMovement.cs](PlayerMovement.cs), [Shoot.cs](Shoot.cs), [PlayerHurt.cs](PlayerHurt.cs)
- 적 계층: [ZombieBase.cs](ZombieBase.cs), [ZombieAttack.cs](ZombieAttack.cs), [ZomSpawn.cs](ZomSpawn.cs)
- 게임/화면 계층: [GameManager.cs](GameManager.cs), [UIManager.cs](UIManager.cs), [PressESC.cs](PressESC.cs), [Camera.cs](Camera.cs)
- 데이터 계층: [GunData.cs](GunData.cs), [ZombieData.cs](ZombieData.cs), [AudioSetting.cs](AudioSetting.cs)

## 파일별 상세 설명

### 1) [LivingEntity.cs](LivingEntity.cs)

- 역할: 생명체 공통 베이스 클래스
- 타입/상속: MonoBehaviour 상속
- 핵심 상태
- startingHealth: 초기 체력
- Health: 현재 체력 (protected set)
- IsDead: 사망 여부 (protected set)
- OnDead: 사망 이벤트
- 주요 메서드
- OnEnable(): 활성화 시 체력/사망 상태 초기화
- OnDamage(float, Vector3, Vector3): 피해 처리, 체력 0 이하 시 Die 호출
- OnHeal(float): 회복 처리, 최대 체력 제한
- Die(): OnDead 이벤트 호출 후 사망 상태 전환
- 사용 관계
- 상속 대상: PlayerHurt, ZombieBase

### 2) [PlayerHurt.cs](PlayerHurt.cs)

- 역할: 플레이어 피격, 사망, 게임오버 처리
- 타입/상속: LivingEntity 상속
- 의존 컴포넌트
- Animator, AudioSource, PlayerInput, Shoot
- GameManager, UIManager
- Slider(hpSlider), Image(hurtOverlay)
- 주요 메서드
- Awake(): 참조 수집, 오디오소스 보정, 오버레이 초기화
- OnEnable() override: 베이스 초기화 + HP 슬라이더 동기화
- Update(): 사망 상태에서 R 키로 재시작
- OnDamage(...) override: 피격 사운드, 피격 플래시, 베이스 데미지 적용
- Die() override: 사망 애니메이션 준비, 조작 비활성화, 게임오버 호출
- PrepareDeathAnimation(): 사망 사운드/트리거 재생
- DisablePlayerControls(): 입력/사격 비활성화
- ShowGameOverUI(): GameManager.EndGame 호출
- ReloadLevel(): 현재 씬 다시 로드
- EnsureDeathAnimation(), PlayHurtFlash(): 코루틴 기반 보조 처리
- 오디오 처리
- AudioSetting.Current.EffectVolume 값을 사용해 피격/사망 효과음 볼륨 제어

### 3) [PlayerMovement.cs](PlayerMovement.cs)

- 역할: 플레이어 이동 및 마우스 조준 회전
- 타입/상속: MonoBehaviour 상속
- 의존 컴포넌트
- PlayerInput, Animator, Rigidbody, Shoot, PlayerHurt
- 주요 메서드
- Awake(): 참조 수집 및 경고/오류 로그
- Update(): 이동 애니메이터 파라미터 갱신, 조준 방향 갱신
- FixedUpdate(): Rigidbody 기반 이동/회전 적용
- UpdateLookRotation(): 카메라 레이와 지면 평면 교차점으로 시선 계산
- 주의 사항
- PlayerHurt.IsDead 체크로 사망 시 이동/회전 중지

### 4) [PlayerInput.cs](PlayerInput.cs)

- 역할: 입력 축/버튼 상태를 수집해 다른 스크립트에 제공
- 타입/상속: MonoBehaviour 상속
- 핵심 프로퍼티
- Move(Vector2), MouseX, MouseY, Fire
- 주요 메서드
- Update(): Horizontal/Vertical, Mouse X/Y, Fire1 입력 반영

### 5) [Shoot.cs](Shoot.cs)

- 역할: 사격, 히트 판정, 트레이서 라인/파티클/사운드 처리
- 타입/상속: MonoBehaviour 상속
- 의존 데이터/컴포넌트
- GunData, ParticleSystem, LineRenderer, AudioSource
- LivingEntity, HitBox
- 주요 메서드
- Awake(): 참조 보정, 렌더러/파티클 초기화
- Update(): 마우스 입력 기반 BeginFire/TickFire/EndFire 실행
- BeginFire(), TickFire(), EndFire(): 사격 상태 전환
- ShowLine(), HideLine(), ToggleLine(): 시각효과 제어
- FireRaycast(): 레이캐스트로 피격 대상 데미지 적용
- PlayGunSound(): 사격 효과음 재생
- 오디오 처리
- AudioSetting.Current.EffectVolume 값을 사용

### 6) [GameManager.cs](GameManager.cs)

- 역할: 점수와 게임오버 상태 총괄
- 타입/상속: MonoBehaviour 상속
- 의존 컴포넌트
- UIManager, ZombieSpawner
- 핵심 상태
- score, IsGameOver
- 주요 메서드
- Awake(): UIManager 참조 수집
- Start(): 초기 점수 UI 반영
- AddScore(int): 점수 증가 및 UI 갱신
- EndGame(): 게임오버 전환, 스포너 중지, 게임오버 UI 활성화

### 7) [UIManager.cs](UIManager.cs)

- 역할: 점수/웨이브/게임오버 UI 및 ESC 메뉴 토글 연동
- 타입/상속: MonoBehaviour 상속
- 의존 컴포넌트
- Text(scoreText, waveText), GameObject(gameOverUI), PressESC
- 주요 메서드
- Awake(): 비활성 오브젝트 포함 참조 탐색
- OnEnable(): UI 초기화, 일시정지 해제, 시간 배율 복구
- Update(): ESC 입력 시 메뉴 토글 시도
- TryTogglePauseMenu(), TogglePauseMenuNextFrame(): 토글 보조
- SetScoreText(int), SetWaveInfo(int): 텍스트 갱신
- SetActiveGameOverUI(bool): 게임오버 패널 활성화/비활성화
- OnclickRestart(): 현재 씬 재시작

### 8) [PressESC.cs](PressESC.cs)

- 역할: 일시정지 메뉴 표시/숨김, BGM/효과음 슬라이더 처리, 종료 버튼 처리
- 타입/상속: MonoBehaviour 상속
- 의존 컴포넌트/데이터
- AudioSetting, AudioSource(bgmSource)
- Slider(BGMSlider, EffectSlider), Button(continueButton, quitButton)
- 핵심 상태
- isMenuOpen, bgmBaseVolume, hasCapturedBgmBaseVolume
- 주요 메서드
- Awake(): 패널/슬라이더/BGM 소스 탐색, 리스너 등록, 초기 볼륨 적용
- OnDestroy(): 리스너 해제
- ShowMenu(), HideMenu(), ToggleMenu(): 메뉴 상태 전환
- SetMenuState(bool): 메뉴 활성화와 Time.timeScale 제어
- SetBgmVolume(float): AudioSetting BGM 값 갱신 + BGM 소스 적용
- SetEffectVolume(float): AudioSetting 효과음 값 갱신
- QuitGame(): 에디터/빌드 환경에 따라 종료 수행
- ResolveMenuPanel(), ResolveSliders(), ResolveBgmSource(): 참조 자동 탐색
- ApplyBgmVolume(): BGM 볼륨 계산 적용
- FindBgmSourceInScene(), FindSliderInScene(), FindInactiveInScene(): 씬 검색 유틸

### 9) [AudioSetting.cs](AudioSetting.cs)

- 역할: 오디오 설정 ScriptableObject
- 타입/상속: ScriptableObject 상속
- 주요 필드
- bgmVolume, effectVolume
- bgmClip
- effectClips (길이 9)
- 주요 정적 상태
- Current: 현재 활성 설정 인스턴스
- 주요 메서드
- RegisterAsCurrent(): Current 등록
- SetBgmVolume(float), SetEffectVolume(float): 볼륨 범위 클램프 및 이벤트 호출
- GetEffectClip(int): 인덱스 기반 효과음 반환
- 이벤트
- BgmVolumeChanged, EffectVolumeChanged

### 10) [Camera.cs](Camera.cs)

- 역할: 플레이어 추적 카메라
- 타입/상속: MonoBehaviour 상속
- 주요 필드
- target, offset
- 주요 메서드
- Awake(): Player 태그 또는 PlayerMovement로 타겟 탐색
- LateUpdate(): target + offset 위치 추적 및 LookAt

### 11) [ZombieBase.cs](ZombieBase.cs)

- 역할: 좀비 상태 머신, 이동, 피격/사망 처리
- 타입/상속: LivingEntity 상속
- 어트리뷰트
- RequireComponent(AudioSource)
- 의존 컴포넌트/데이터
- ZombieData, ParticleSystem, AudioClip
- Animator, AudioSource, Collider, NavMeshAgent, ZombieAttack
- 상태
- Status enum: Idle, Move, Death
- currentStatus, deathStartTime
- 주요 메서드
- Awake(): 참조 수집, 에이전트 검증, 파티클 보정, 데이터 적용
- OnEnable() override: 상태 초기화, 네비 재배치, 타겟 탐색
- Update(): 상태별 UpdateIdle/UpdateMove/UpdateDie 실행
- Setup(ZombieData): 런타임 데이터 주입
- ApplyZombieData(): 체력/속도/공격력 반영
- SetStatus(Status): 상태 전환 및 애니메이션/에이전트 제어
- FindPlayerTarget(), IsTargetValid(): 타겟 유효성 확인
- UpdateIdle(), UpdateMove(), UpdateDie(): 상태별 동작
- StartSinking(): 가라앉기 시작 시점 조정
- Die() override, OnDamage(...) override: 사망/피격 오버라이드
- PlayZombieSound(AudioClip): 효과음 재생
- 오디오 처리
- AudioSetting.Current.EffectVolume 값 사용

### 12) [ZombieAttack.cs](ZombieAttack.cs)

- 역할: 플레이어 근접 공격 처리
- 타입/상속: MonoBehaviour 상속
- 의존 컴포넌트
- ZombieBase, PlayerHurt
- 주요 메서드
- Awake(): ZombieBase 참조 검증
- SetAttackDamage(float): 공격력 주입
- OnTriggerEnter/Stay, OnCollisionEnter/Stay: 충돌 이벤트를 TryAttack으로 통합
- TryAttack(Collider): 공격 간격, 대상 유효성, 데미지 적용

### 13) [ZomSpawn.cs](ZomSpawn.cs)

- 역할: 웨이브 기반 좀비 생성 및 웨이브/점수 연동
- 타입/상속: MonoBehaviour 상속
- 클래스명: ZombieSpawner
- 의존 요소
- GameManager, UIManager
- ZombieBase 배열 Prefab, ZombieData 배열 zombieDatas
- Transform 배열 spawnPoints
- 주요 메서드
- Awake(): 매니저 참조 수집
- Update(): 활성 좀비 리스트가 비면 SpawnWave 실행
- SpawnWave(): 웨이브 증가, 수량 계산 후 반복 생성
- CreateZombie(): 스폰 포인트 선택, 프리팹 생성, 데이터 매핑, OnDead 리스너 등록
- 구현 특징
- Prefab 과 zombieDatas 길이 불일치 시 경고 로그
- OnDead 이벤트로 리스트 제거, 점수 증가, 오브젝트 파괴 예약

### 14) [ZombieData.cs](ZombieData.cs)

- 역할: 좀비 스탯 데이터 ScriptableObject
- 타입/상속: ScriptableObject 상속
- 데이터 필드
- maxHP, damage, Speed
- 사용 위치
- ZombieBase.Setup, ZombieBase.ApplyZombieData

### 15) [IZombieStats.cs](IZombieStats.cs)

- 역할: 좀비 스탯 인터페이스 정의
- 타입/상속: 인터페이스
- 멤버
- MaxHp, MoveSpeed
- 현재 상태
- 프로젝트 내 실제 구현체는 없음

### 16) [GunData.cs](GunData.cs)

- 역할: 무기 데이터 ScriptableObject
- 타입/상속: ScriptableObject 상속
- 데이터 필드
- shotClip, damage
- 사용 위치
- Shoot.PlayGunSound, Shoot.FireRaycast

### 17) [HitBox.cs](HitBox.cs)

- 역할: 충돌 콜라이더 기록 컨테이너
- 타입/상속: MonoBehaviour 상속
- 주요 필드
- colliders 리스트, Colliders 프로퍼티
- 주요 메서드
- OnTriggerEnter(Collider): 신규 콜라이더 등록
- OnTriggerExit(Collider): 콜라이더 제거

## 상속 및 관계 다이어그램

- LivingEntity
- PlayerHurt : LivingEntity
- ZombieBase : LivingEntity

## 주요 호출 흐름

- 입력 흐름
- PlayerInput.Update → PlayerMovement.FixedUpdate, Shoot.Update
- 전투 흐름
- Shoot.FireRaycast → LivingEntity.OnDamage
- LivingEntity.OnDamage(체력 0 이하) → Die → OnDead 이벤트
- 좀비 생성 흐름
- ZombieSpawner.Update(좀비 0) → SpawnWave → CreateZombie
- UI/일시정지 흐름
- UIManager.Update(ESC) → PressESC.ToggleMenu → Time.timeScale 제어
- 오디오 흐름
- PressESC.SetBgmVolume/SetEffectVolume → AudioSetting 반영
- Shoot/PlayerHurt/ZombieBase 효과음 재생 시 AudioSetting.Current.EffectVolume 사용

## 참고

- 클래스명과 파일명이 다른 경우
- 파일 [ZomSpawn.cs](ZomSpawn.cs) 안 클래스명은 ZombieSpawner 입니다.
- 현재 인터페이스 [IZombieStats.cs](IZombieStats.cs) 는 선언만 있고 구현체가 없습니다.
