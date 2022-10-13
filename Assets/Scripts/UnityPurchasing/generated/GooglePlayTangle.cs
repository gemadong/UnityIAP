// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("adtYe2lUX1Bz3xHfrlRYWFhcWVqg7aUGAg7qJmAmMPC0gMMD2V/Fd9+ztVr8w6tEhKRZXWK1hbhx3/aa21hWWWnbWFNb21hYWfIvlXJpASrriusXnJu3Krr+DTdzbSSKMHhtMtPiUwPf0Fs+yM+2om/QZP7yjJO9q8HC6pgBwyn+SqJwyv0j50wAHAiJnoYIA+gAR+VyR0fdbS1kU2mDHnhbmhlqOA4pyQxqVgR6scXLNlupPA0kbmBzenRVKjidDc8ghCfNuf9PYBVYJREjIuyAiCRq5oKgdcwsUcB+C0M3baL8zFyFq7lw6N4PV3PRGDLpS7yfTtXjezD/AJHbJbGbxOiCg93pjg8nEDya7VPNet2oXLER+UapVE1W9yYUNltaWFlY");
        private static int[] order = new int[] { 0,3,4,3,10,8,9,12,13,10,12,12,12,13,14 };
        private static int key = 89;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
