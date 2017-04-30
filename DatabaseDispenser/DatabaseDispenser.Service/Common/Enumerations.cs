using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseDispenser.Service.Common
{

    public enum DatabaseType
    {
        [Description("L")]
        Used,
        [Description("NL")]
        Open,
        [Description("BC")]
        BeingCreated
    }

    public enum ServiceFabricUrls
    {
        [Description("fabric:/MultiBannerBackendV5/StoreActorService")]
        StoreActor,
        [Description("fabric:/MultiBannerBackendV5/PostActorService")]
        PostActor,
        [Description("fabric:/MultiBannerBackendV5/StoreLocator")]
        StoreLocatorExternal,
        [Description("fabric:/MultiBannerBackendV5/Logging")]
        Logging,
        [Description("fabric:/MultiBannerBackendV5/UserActorService")]
        UserActor,
        [Description("fabric:/MultiBannerBackendV5/ProfileActorService")]
        ProfileActor,
        [Description("fabric:/MultiBannerBackendV5/SurveysActorService")]
        SurveyActor,
        [Description("fabric:/MultiBannerBackendV5/LoginActorService")]
        LoginActor,
        [Description("fabric:/MultiBannerBackendV5/FileUploadActorService")]
        FileUpload,
        [Description("fabric:/MultiBannerBackendV5/ContentActorService")]
        ContentActor,
        [Description("fabric:/MultiBannerBackendV5/Logout")]
        Logout,
        [Description("fabric:/MultiBannerBackendV5/EmailActorService")]
        EmailActor,
        [Description("fabric:/MultiBannerBackendV5/CustomerActorService")]
        CustomerActor,
        [Description("fabric:/MultiBannerBackendV5/NotificationActorService")]
        NotificationActor,
        [Description("fabric:/MultiBannerBackendV5/DepartmentActorService")]
        DepartmentActor,
        [Description("fabric:/MultiBannerBackendV5/ContactActorService")]
        ContactActor,
        [Description("fabric:/MultiBannerBackendV5/EmailService")]
        Email,
        [Description("fabric:/MultiBannerBackendV5/InsightsLogging")]
        InsightsLogging,
        [Description("fabric:/MultiBannerBackendV5/MandrillEmail")]
        MandrillEmail,
        [Description("fabric:/MultiBannerBackendV5/SilverpopEmail")]
        SilverpopEmail,
        [Description("fabric:/MultiBannerBackendV5/ReliableDictionaryCache")]
        ReliableDictionaryCache,
        [Description("fabric:/MultiBannerBackendV5/StoreRepository")]
        StoreRepository,
        [Description("fabric:/MultiBannerBackendV5/PostRepository")]
        PostRepository,
        [Description("fabric:/MultiBannerBackendV5/DepartmentRepository")]
        DepartmentRepository,
        [Description("fabric:/MultiBannerBackendV5/ContentRepository")]
        ContentRepository,
        [Description("fabric:/MultiBannerBackendV5/ContactRepository")]
        ContactRepository,
        [Description("fabric:/MultiBannerBackendV5/ServiceNow")]
        ServiceNow,
        [Description("fabric:/MultiBannerBackendV5/ProfileRepository")]
        ProfileRepository,
        [Description("fabric:/MultiBannerBackendV5/SurveyRepository")]
        SurveyRepository,
        [Description("fabric:/MultiBannerBackendV5/FacebookGraphApi")]
        FacebookRepository,
        [Description("fabric:/MultiBannerBackendV5/NotificationRepository")]
        NotificationRepository,
        [Description("fabric:/MultiBannerBackendV5/AzureApi")]
        AzureRepository,
        [Description("fabric:/MultiBannerBackendV5/PushNotification")]
        PushNotification,
        [Description("fabric:/MultiBannerBackendV5/Localytics")]
        Localytics,
        [Description("fabric:/MultiBannerBackendV5/GiftCardActorService")]
        GiftCardActor,
        [Description("fabric:/MultiBannerBackendV5/GiftCardRepository")]
        GiftCardRepository,
        [Description("fabric:/MultiBannerBackendV5/PaymentGateway")]
        PaymentGatewayRepository
    }

    public enum DocumentDbCollections
    {
        [Description("store")]
        Store,
        [Description("user")]
        User,
        [Description("post")]
        Post,
        [Description("fileupload")]
        FileUpload,
        [Description("content")]
        Content,
        [Description("department")]
        Department,
        [Description("contact")]
        Contact,
        [Description("survey")]
        Survey,
        [Description("surveyanswer")]
        SurveyAnswer,
        [Description("notification")]
        Notification,
        [Description("storedvaluecards")]
        StoredValueCards,
        [Description("settings")]
        settings
    }

    public enum ServiceStatus
    {
        Success,
        Failed,
        Created,
        Deleted,
        InvalidRequest,
        DataNotFound,
        Exception,
        Accepted,
        Gone,
        BadRequest,
        Unauthorized,
        NoContent,
        PreCondition,
        CardInactive,
        TooManyInquries,
        BadGateway
    }

    public enum LoginIdentifierType
    {
        UserId,
        Email,
        UserName,
        Facebook,
        ActivationToken,
        PasswordResetToken
    }

    public enum EmailTemplates
    {
        [Description("AdminReviewCommentTemplate")]
        ReviewComment,
        [Description("AdminFlaggedPostTemplate")]
        FlaggedPost,
        [Description("AdminFlaggedCommentTemplate")]
        FlaggedComment,
        [Description("AdminDailyReportTemplate")]
        AdminDailyReport
    }

    public enum ContextKeys
    {
        CorrelationId
    }

    public enum Severity
    {
        Critical,
        Warning,
        Information,
        Verbose
    }

    public enum Device
    {
        [Description("ios")]
        Ios,
        [Description("android")]
        Android
    }

    public enum DependencyKind
    {
        StoreLocator,
        DocumentDb,
        AzureStorage,
        ReliableDictionary,
        Localytics,
        Actor,
        Api,
        PaymentGateway,
        FacebookGraph
    }

    public enum DependencyResult
    {
        Timeout,
        Success,
        Failed
    }
    public enum ExternalService
    {
        DocumentDb,
        Mandrill,
        StoreLocator,
        AzureBlobStorage,
        FacebookGraphApi,
        ServiceNow
    }

    public enum QuestionType
    {
        Text,
        Choice
    }

    public enum ReceiverType
    {
        Store,
        User
    }

    // This sequence shold not be changed
    public enum ContentType
    {
        Survey = 0,
        Post = 1,
        ClusteredStorePosts,
        EReceipt
    }

    // This sequence shold not be changed
    public enum NotificationType
    {
        Survey = 0,
        Post = 1,
        ClusteredStorePosts
    }

    public enum NotificationEventType
    {
        SurveyCreated = 0,
        PostCreated = 1,
        CommentCreated = 2
    }

    public enum UserNotificationType
    {
        [Description("comments-my-post")]
        NotifyOnCommentsOnMyPost,
        [Description("comments-other-post")]
        NotfifyOnCommentsOnOtherPost,
        [Description("official-updates")]
        NotfifyOnOfficialUpdates,
        [Description("initial-survey")]
        NotifyOnInitialSurvey,
        [Description("clustered-store-posts")]
        ClusteredStorePosts,
        [Description("tjmaxx-official-communications")]
        TjMaxxOfficialCommunications
    }
    public enum UserSVCPreferenceType
    {
        [Description("svc-password-enabled")]
        SVCPasswordEnbled,
        [Description("svc-touchid-enabled")]
        SVCTouchIdEnabled,
    }

    public enum UserType
    {
        Customer,
        Influencer,
        Associate
    }

    public enum StoreType
    {
        [Description("HomeGoods")]
        HomeGoods = 28,
        [Description("Marshalls")]
        Marshalls = 10,
        [Description("TJMaxx")]
        TJMaxx = 08,
        [Description("STP")]
        STP = 50
    }



    public enum UserRoles
    {
        [Description("admin")]
        Admin,
        [Description("customer")]
        Customer,
        [Description("associate")]
        Associate,
        [Description("influencer")]
        Influencer
    }

    public enum LoginType
    {
        [Description("C")]
        Customer,
        [Description("F")]
        Facebook,
        [Description("A")]
        Admin,
        [Description("S")]
        Associate,
        [Description("0")]
        Invalid,
        [Description("store")]
        Store
    }
    public enum JwtPayloadTypes
    {
        [Description("userId")]
        UserId,
        [Description("role")]
        UserRole,
        [Description("userLogin")]
        UserLogin,
        [Description("exp")]
        Expiry,
        [Description("storeId")]
        StoreId,
        [Description("userName")]
        DisplayName,
        [Description("pictureUrl")]
        PictureUrl
    }

    public enum TimezoneOptions
    {
        [Description("-05:00")]
        Est = -5,
        [Description("-06:00")]
        Cst = -6,
        [Description("-07:00")]
        Mst = -7,
        [Description("-08:00")]
        Pst = -8
    }

    public enum BannerType
    {
        [Description("homegoods")]
        Homegoods = 0,
        [Description("tjmaxx")]
        TjMaxx = 1
    }

    public enum GiftCardType
    {
        [Description("gift-card")]
        GiftCard,
        [Description("e-gift-card")]
        EGiftCard,
        [Description("reward-certificate")]
        RewardCrtificate

    }

}
