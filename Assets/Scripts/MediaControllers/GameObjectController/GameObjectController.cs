using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CuttingRoom.Exceptions;
using System;

namespace CuttingRoom
{
    public class GameObjectController : MediaController
    {
        /// <summary>
        /// The GameObject spawned by this controller.
        /// </summary>
        private GameObject instantiatedGameObject = null;
        
        public override void Preload(NarrativeSpace narrativeSpace, AtomicNarrativeObject atomicNarrativeObject)
        {
            base.Preload(narrativeSpace, atomicNarrativeObject);

            MediaSource mediaSource = atomicNarrativeObject.mediaSource;

            if (mediaSource.mediaSourceData.objects.Count == 0)
            {
                throw new InvalidMediaException("No GameObject to be instantiated is defined.");
            }

            if (mediaSource.mediaSourceData.objects[0] as GameObject == null)
            {
                throw new InvalidMediaException("The object specified is not a GameObject so cannot be instantiated.");
            }

            // Instantiate and parent to the specified transform in the scene.
            instantiatedGameObject = Instantiate(mediaSource.mediaSourceData.objects[0] as GameObject, atomicNarrativeObject.mediaParent);

            instantiatedGameObject.SetActive(false);
        }

        public override void Play(NarrativeSpace narrativeSpace, AtomicNarrativeObject atomicNarrativeObject)
        {
            instantiatedGameObject.SetActive(true);
        }

        public override void Shutdown(Action onShutdownComplete)
        {
            Destroy(instantiatedGameObject);

            base.Shutdown(onShutdownComplete);
        }
    }
}
