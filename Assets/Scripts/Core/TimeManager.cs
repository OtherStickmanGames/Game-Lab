using System;
using UnityEngine;

namespace DwarfClone.Core
{
    public class TimeManager : MonoBehaviour
    {
        public static TimeManager Instance { get; private set; }

        public event Action OnTick;
        public event Action<int, int, int> OnTimeChanged; // day, hour, minute
        public event Action<float> OnSpeedChanged;

        [Header("Simulation State")]
        [SerializeField] private float currentSpeed = 1.0f;
        [SerializeField] private bool isPaused = false;

        private float tickTimer = 0f;
        private float inGameMinuteTimer = 0f;
        private int currentDay = 1;
        private int currentHour = 8;
        private int currentMinute = 0;

        public float CurrentSpeed => isPaused ? 0f : currentSpeed;
        public bool IsPaused => isPaused;
        public int Day => currentDay;
        public int Hour => currentHour;
        public int Minute => currentMinute;
        public string FormattedTime => $"Day {currentDay:00} - {currentHour:00}:{currentMinute:00}";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            // Hotkeys for time speed
            if (Input.GetKeyDown(KeyCode.Space))
            {
                TogglePause();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SetSpeed(1.0f);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SetSpeed(2.0f);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SetSpeed(5.0f);
            }

            if (isPaused) return;

            float dt = Time.deltaTime * currentSpeed;

            // Tick processing
            tickTimer += dt;
            if (tickTimer >= Constants.BASE_TICK_RATE)
            {
                tickTimer -= Constants.BASE_TICK_RATE;
                OnTick?.Invoke();
            }

            // In-game clock
            inGameMinuteTimer += dt;
            if (inGameMinuteTimer >= (Constants.BASE_TICK_RATE / Constants.MINUTES_PER_TICK))
            {
                inGameMinuteTimer = 0f;
                currentMinute++;
                if (currentMinute >= 60)
                {
                    currentMinute = 0;
                    currentHour++;
                    if (currentHour >= Constants.HOURS_PER_DAY)
                    {
                        currentHour = 0;
                        currentDay++;
                    }
                }
                OnTimeChanged?.Invoke(currentDay, currentHour, currentMinute);
            }
        }

        public void SetSpeed(float speed)
        {
            currentSpeed = Mathf.Max(0.5f, speed);
            isPaused = false;
            Time.timeScale = currentSpeed;
            OnSpeedChanged?.Invoke(CurrentSpeed);
        }

        public void TogglePause()
        {
            isPaused = !isPaused;
            Time.timeScale = isPaused ? 0f : currentSpeed;
            OnSpeedChanged?.Invoke(CurrentSpeed);
        }

        public void Pause()
        {
            isPaused = true;
            Time.timeScale = 0f;
            OnSpeedChanged?.Invoke(0f);
        }

        public void Resume()
        {
            isPaused = false;
            Time.timeScale = currentSpeed;
            OnSpeedChanged?.Invoke(currentSpeed);
        }
    }
}
