# 🌿 자연의 신비 모험 (Nature's Mystery Adventure)

> 초등학교 1학년을 위한
> **3D 인터랙티브 자연 관찰 학습 콘텐츠**

“자연의 신비 모험”은 아이들이 **동물의 소리를 듣고 동물을 맞히는 활동**과 **식물의 성장 단계를 올바른 순서로 조합하는 활동**을 통해 자연에 대한 흥미와 이해를 키울 수 있도록 설계된 **미션 기반 학습 콘텐츠**입니다.

---

## 📁 프로젝트 구조

```
Assets/
├── Scripts/
│   ├── Core/              # GameManager, SettingManager, SoundManager 등 전반적 흐름 제어
│   ├── UI/                # IntroManager, Mission1_UIManager, Mission2_UIManager
│   ├── Mission1/          # 동물 소리 맞히기 미션 로직
│   ├── Mission2/          # 식물 성장 조합 미션 로직
│   ├── Interaction/       # TouchObjectDetector, TouchSelf, WordEnter 등 입력 감지
│   └── Utility/           # CoroutineRunner, StringKeys, BlurFeature 등 공용 기능
```

---

## 🔁 실행 흐름 요약

### 🎬 1. 인트로 씬 (`IntroManager`)

* 웨이브 애니메이션 타이틀 출력
* `"터치하여 시작"` 텍스트 반복 점멸
* ▶️ 시작 클릭 시 → Mission1 시작

---

### 🐾 2. Mission1 - 동물 소리 맞히기

* `Mission1_DataManager`가 랜덤으로 동물 사운드 퀴즈 생성
* 유저는 정답이라고 생각하는 동물을 터치
* 정답 시 `"딩동댕~ 정답이에요!"` 나레이션 출력
* 5문제 완료 시 Mission2로 전환

---

### 🌱 3. Mission2 - 식물 성장 조합하기

* `Mission2_DataManager`가 5단계 성장 순서 (씨앗 → 익은 토마토) 랜덤 생성
* 드래그 앤 드롭으로 순서대로 성장 과정을 배치
* 정답일 경우 애니메이션 및 `"정확히 알고 있네요!"` 나레이션 출력
* 5단계 완료 시 미션 종료 및 엔딩 나레이션 출력

---

## 📚 학습 콘텐츠 구성

| 미션        | 주제            | 주요 학습 요소            | 사용자 인터랙션          |
| --------- | ------------- | ------------------- | ----------------- |
| Mission 1 | 동물 소리 맞히기     | 동물 이름과 소리의 연결       | 동물 캐릭터 터치         |
| Mission 2 | 식물 성장 단계 조합하기 | 식물의 성장 순서화 및 기억력 훈련 | 컬러 식물 조각 드래그 앤 드롭 |

* **오디오 중심 학습**: 동물 소리 및 나레이션은 `SoundManager`를 통해 재생
* **감정 피드백**: `npcAnimator`를 통해 정답/오답 시 애니메이션 반응 제공
---
## 🖼️ 예시 이미지

### Mission01 - 동물 소리 맞히기

| 미션 시작 화면 1                                           | 미션 시작 화면 2                                          | 정답 시 효과                                           |
| ------------------------------------------------ | ------------------------------------------------ | ------------------------------------------------ |
| ![](/ScreenShots/Screenshot_20250416_091137.jpg) | ![](/ScreenShots/Screenshot_20250416_091134.jpg) | ![](/ScreenShots/Screenshot_20250416_091155.jpg) |

---

### Mission02 - 식물 성장 단계 조합하기

| 미션 시작 화면                                            | 정답 시 효과                                          | 오답 시 화면                                          |
| ------------------------------------------------ | ------------------------------------------------ | ------------------------------------------------ |
| ![](/ScreenShots/Screenshot_20250416_091220.jpg) | ![](/ScreenShots/Screenshot_20250416_091233.jpg) | ![](/ScreenShots/Screenshot_20250416_091259.jpg) |

---
## 🔧 주요 클래스 설명

| 클래스                      | 역할 및 기능                                 |
| ------------------------ | --------------------------------------- |
| ✅ `GameManager`          | 전반적인 미션 흐름 제어, NPC 연출 및 정답 상태 관리        |
| ✅ `NarrationManager`     | 나레이션 텍스트 출력 + 오디오 동기화 + 타이핑 애니메이션       |
| ✅ `CoroutineRunner`      | 키 기반 코루틴 실행 및 중복 방지 / 타임아웃 처리           |
| ✅ `TouchObjectDetector`  | 입력 감지 처리 (Mission1: 터치, Mission2: 드래그)  |
| ✅ `Mission1_DataManager` | 동물 리스트 셔플, 문제 구성 및 정답 설정                |
| ✅ `Mission2_DataManager` | 성장 순서 셔플, 정답 검증, 나레이션 호출 등              |
| ✅ `Mission1_UIManager`   | 타이틀 텍스트 출력, 정오답 반응 처리, 다음 문제 진행         |
| ✅ `Mission2_UIManager`   | 성장 과정 이미지 연출, 팝업 등장 애니메이션, 최종 엔딩 흐름 제어  |
| ✅ `WordEnter`            | 드래그 도착지에서 정답 여부 확인 (Mission2 전용)        |
| ✅ `TouchSelf`            | 동물 오브젝트에 부착, 정답/오답 이벤트 발생 (Mission1 전용) |

---

## ⚙️ 실행 환경

* **Unity 버전**: 2022.3 LTS 이상
* **지원 디바이스**: Leia Lume Pad 2 (Android)
* **사용 패키지**:

  * DOTween – UI 애니메이션
  * TextMeshPro – 텍스트 출력
  * LeiaUnity – 3D 디스플레이 지원

---

## ✨ 보조 기능

* `SettingManager`: BGM/SFX 설정, 3D 모드 토글, 재시작/종료 버튼 포함
* `AnimalAnimatorLayerSelector`: 동물별 애니메이터 레이어를 자동으로 활성화
* `FloatObject`: 배경 오브젝트에 부유감 애니메이션 적용
* `StringKeys`: 미션 키, 태그, 오디오 클립 이름 등 문자열 상수 관리

