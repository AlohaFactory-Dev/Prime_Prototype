#import <Foundation/Foundation.h>
@import FlamingoKit;

static NSString *const METHOD_HANDLE_FLAMINGO_LOG = @"_handleFlamingoLog";

#pragma mark Utility Methods

NSString *convertCString(const char *string) {
    if (string)
        return [NSString stringWithUTF8String:string];
    else
        return nil;
}

char *makeStringCopy(NSString *nstring) {
    if ((!nstring) || (nil == nstring) || (nstring == (id) [NSNull null]) || (0 == nstring.length))
        return NULL;

    const char *string = [nstring UTF8String];

    if (string == NULL)
        return NULL;

    char *res = (char *) malloc(strlen(string) + 1);
    strcpy(res, string);

    return res;
}

#pragma mark ALFlamingo Wrapper
@interface ALFlamingoDelegate : NSObject
@property(nonatomic) NSString *gameObject;
@end

@implementation ALFlamingoDelegate
- (void) setupFlamingo:(NSString *)gameObject
             accessKey:(NSString *)accessKey
            appVersion:(nullable NSString *)appVersion {
    self.gameObject = gameObject;
    [ALFlamingo configureWithAccessKey:accessKey
                         appVersion:appVersion];
    [self sendFlamingoUnityLogMessage:@"FlamingoWrapper.setup" method:METHOD_HANDLE_FLAMINGO_LOG];
}

- (void) login:(NSString *)appUserId {
    [ALFlamingoCommon loginWithAppUserId:appUserId];
    [self sendFlamingoUnityLogMessage:@"FlamingoWrapper.login" method:METHOD_HANDLE_FLAMINGO_LOG];
}

- (void) logEvent:(NSString *)eventCode
        eventData:(nullable NSString *)eventData
  customEventCode:(nullable NSString *)customEventCode {
    [ALFlamingoCommon logEventWithEventCode:eventCode
                                  eventData:eventData
                            customEventCode:customEventCode];
    [self sendFlamingoUnityLogMessage:@"FlamingoWrapper.logEvent" method:METHOD_HANDLE_FLAMINGO_LOG];
}

- (void) startPlaySession:(NSString *)playSessionInfo
             customParams:(nullable NSString *)customParams {
    [ALFlamingoCommon startPlaySessionWithSessionInfo:playSessionInfo
                                         customParams:customParams
                                           completion:^(NSString * _Nonnull message) {
        [self sendFlamingoUnityLogMessage:message method:METHOD_HANDLE_FLAMINGO_LOG];
    }];
}

- (void) endPlaySession:(NSString *)result
           customParams:(nullable NSString *)customParams {
    [ALFlamingoCommon endPlaySessionWithResult:result
                                  customParams:customParams
                                    completion:^(NSString * _Nonnull message) {
        [self sendFlamingoUnityLogMessage:message method:METHOD_HANDLE_FLAMINGO_LOG];
    }];
}

- (void) finishTutorial:(NSString *)tutorialId
           tutorialName:(nullable NSString *)tutorialName
           customParams:(nullable NSString *)customParams {
    [ALFlamingoCommon finishTutorialWithTutorialId:tutorialId
                                      tutorialName:tutorialName
                                      customParams:customParams
                                        completion:^(NSString * _Nonnull message) {
        [self sendFlamingoUnityLogMessage:message method:METHOD_HANDLE_FLAMINGO_LOG];
    }];
}

- (void) purchaseIAP:(NSString *)store
         isoCurrency:(NSString *)isoCurrency
               price:(double)price
       transactionId:(nullable NSString *)transactionId
              itemId:(nullable NSString *)itemId
            itemName:(nullable NSString *)itemName
              isTest:(BOOL) isTest
        customParams:(nullable NSString *)customParams {
    [ALFlamingoCommon purchaseIAPWithStore:store
                               isoCurrency:isoCurrency
                                     price:price
                             transactionId:transactionId
                                    itemId:itemId
                                  itemName:itemName
                                    isTest:isTest
                              customParams:customParams
                                completion:^(NSString * _Nonnull message) {
        [self sendFlamingoUnityLogMessage:message method:METHOD_HANDLE_FLAMINGO_LOG];
    }];
}

- (void) changeAssetAmount:(NSString *)assetId
                 assetName:(nullable NSString *)assetName
                amountDiff:(NSInteger) amountDiff
              resultAmount:(NSInteger) resultAmount
                  actionId:(nullable NSString *)actionId
                actionName:(nullable NSString *)actionName
                  objectId:(nullable NSString *)objectId
                objectName:(nullable NSString *)objectName
              customParams:(nullable NSString *)customParams {
    [ALFlamingoCommon changeAssetAmountWithAssetId:assetId
                                         assetName:assetName
                                        amountDiff:amountDiff
                                      resultAmount:resultAmount
                                          actionId:actionId
                                        actionName:actionName
                                          objectId:objectId
                                        objectName:objectName
                                      customParams:customParams];
}

- (void) changeItemAmount:(NSString *)itemId
                 itemName:(nullable NSString *)itemName
               amountDiff:(NSInteger) amountDiff
             resultAmount:(NSInteger) resultAmount
                 actionId:(nullable NSString *)actionId
               actionName:(nullable NSString *)actionName
                 objectId:(nullable NSString *)objectId
               objectName:(nullable NSString *)objectName
             customParams:(nullable NSString *)customParams {
    [ALFlamingoCommon changeItemAmountWithItemId:itemId
                                        itemName:itemName
                                      amountDiff:amountDiff
                                    resultAmount:resultAmount
                                        actionId:actionId
                                      actionName:actionName
                                        objectId:objectId
                                      objectName:objectName
                                    customParams:customParams];
}


#pragma mark Helper Methods

- (void) sendFlamingoUnityLogMessage:(NSString *)message
                              method:(NSString *)method {
    UnitySendMessage([self.gameObject UTF8String], [method UTF8String], [message UTF8String]);
}

@end


#pragma mark Bridging Methods
static ALFlamingoDelegate *_ALFlamingoDelegate;

static ALFlamingoDelegate *_ALFlamingoDelegateShared() {
    if (_ALFlamingoDelegate == nil) {
        _ALFlamingoDelegate = [[ALFlamingoDelegate alloc] init];
    }
    return _ALFlamingoDelegate;
}

void _ALFlamingoSetup(const char* gameObject, const char* accessKey, const char* appVersion) {
    [_ALFlamingoDelegateShared() setupFlamingo:convertCString(gameObject)
                                     accessKey:convertCString(accessKey)
                                    appVersion:convertCString(appVersion)];
}

void _ALFlamingoLogin(const char* appUserId) {
    [_ALFlamingoDelegateShared() login:convertCString(appUserId)];
}

void _ALFlamingoLogEvent(const char* eventCode, const char* eventData, const char* customEventCode) {
    [_ALFlamingoDelegateShared() logEvent:convertCString(eventCode)
                                eventData:convertCString(eventData)
                          customEventCode:convertCString(customEventCode)];
}

void _ALFlamingoStartPlaySession(const char* playSessionInfo, const char* customParams) {
    [_ALFlamingoDelegateShared() startPlaySession:convertCString(playSessionInfo)
                                     customParams:convertCString(customParams)];
}

void _ALFlamingoEndPlaySession(const char* result, const char* customParams) {
    [_ALFlamingoDelegateShared() endPlaySession:convertCString(result)
                                   customParams:convertCString(customParams)];
}

void _ALFlamingoFinishTutorial(const char* tutorialId, const char* tutorialName, const char* customParams) {
    [_ALFlamingoDelegateShared() finishTutorial:convertCString(tutorialId)
                                   tutorialName:convertCString(tutorialName)
                                   customParams:convertCString(customParams)];
}

void _ALFlamingoPurchaseIAP(const char* store,
                            const char* isoCurrency,
                            const double price,
                            const char* transactionId,
                            const char* itemId,
                            const char* itemName,
                            const bool isTest,
                            const char* customParams) {
    [_ALFlamingoDelegateShared() purchaseIAP:convertCString(store)
                                 isoCurrency:convertCString(isoCurrency)
                                       price:price
                               transactionId:convertCString(transactionId)
                                      itemId:convertCString(itemId)
                                    itemName:convertCString(itemName)
                                      isTest:isTest
                                customParams:convertCString(customParams)];
}

void _ALFlamingoChangeAssetAmount(const char* assetId,
                                  const char *assetName,
                                  const int amountDiff,
                                  const int resultAmount,
                                  const char* actionId,
                                  const char* actionName,
                                  const char* objectId,
                                  const char* objectName,
                                  const char* customParams) {
    [_ALFlamingoDelegateShared() changeAssetAmount:convertCString(assetId)
                                         assetName:convertCString(assetName)
                                        amountDiff:amountDiff
                                      resultAmount:resultAmount
                                          actionId:convertCString(actionId)
                                        actionName:convertCString(actionName)
                                          objectId:convertCString(objectId)
                                        objectName:convertCString(objectName)
                                      customParams:convertCString(customParams)];
}

void _ALFlamingoChangeItemAmount(const char* itemId,
                                 const char *itemName,
                                 const int amountDiff,
                                 const int resultAmount,
                                 const char* actionId,
                                 const char* actionName,
                                 const char* objectId,
                                 const char* objectName,
                                 const char* customParams) {
    [_ALFlamingoDelegateShared() changeItemAmount:convertCString(itemId)
                                         itemName:convertCString(itemName)
                                       amountDiff:amountDiff
                                     resultAmount:resultAmount
                                         actionId:convertCString(actionId)
                                       actionName:convertCString(actionName)
                                         objectId:convertCString(objectId)
                                       objectName:convertCString(objectName)
                                     customParams:convertCString(customParams)];
}
