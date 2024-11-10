﻿namespace AgoraIO.Media
{
    public class ChatTokenBuilder2
    {
        /**
        * Build the CHAT user token.
        *
        * @param appId:          The App ID issued to you by Agora. Apply for a new App ID from
        *                        Agora Dashboard if it is missing from your kit. See Get an App ID.
        * @param appCertificate: Certificate of the application that you registered in
        *                        the Agora Dashboard. See Get an App Certificate.
        * @param userId:         The user's id, must be unique.
        * @param expire:         represented by the number of seconds elapsed since now. If, for example, you want to access the
        *                        Agora Service within 10 minutes after the token is generated, set expire as 600(seconds).
        * @return The Chat User token.
        */
        public static string buildUserToken(string appId, string appCertificate, string userId, uint expire)
        {
            AccessToken2 accessToken = new AccessToken2(appId, appCertificate, expire);
            AccessToken2.Service serviceChat = new AccessToken2.ServiceChat(userId);

            serviceChat.addPrivilegeChat(AccessToken2.PrivilegeChatEnum.PRIVILEGE_CHAT_USER, expire);
            accessToken.addService(serviceChat);

            return accessToken.build();
        }

        /**
         * Build the CHAT app token.
         *
         * @param appId:          The App ID issued to you by Agora. Apply for a new App ID from
         *                        Agora Dashboard if it is missing from your kit. See Get an App ID.
         * @param appCertificate: Certificate of the application that you registered in
         *                        the Agora Dashboard. See Get an App Certificate.
         * @param expire:         represented by the number of seconds elapsed since now. If, for example, you want to access the
         *                        Agora Service within 10 minutes after the token is generated, set expire as 600(seconds).
         * @return The Chat App token.
         */
        public static string buildAppToken(string appId, string appCertificate, uint expire)
        {
            AccessToken2 accessToken = new AccessToken2(appId, appCertificate, expire);
            AccessToken2.Service serviceChat = new AccessToken2.ServiceChat();

            serviceChat.addPrivilegeChat(AccessToken2.PrivilegeChatEnum.PRIVILEGE_CHAT_APP, expire);
            accessToken.addService(serviceChat);

            return accessToken.build();
        }
    }
}