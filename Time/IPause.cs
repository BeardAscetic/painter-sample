using UnityEngine;

namespace BeardAscetic
{
    public interface IPause
    {
        public void RegisterPause();
        public void PauseTime();

        public void ResumeTime();
    }
}
