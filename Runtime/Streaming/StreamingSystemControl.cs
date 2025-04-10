using NaughtyAttributes;
using Readymade.Persistence;
using UnityEngine;

namespace App.Core.Streaming
{
    public class StreamingSystemControl : MonoBehaviour
    {
        [Required]
        [SerializeField]
        private PackSystem packSystem;

        [Required]
        [SerializeField]
        private StreamingSystem streamingSystem;

        private void Update()
        {
            streamingSystem.SetActive(!packSystem.IsPacking);
        }
    }
}