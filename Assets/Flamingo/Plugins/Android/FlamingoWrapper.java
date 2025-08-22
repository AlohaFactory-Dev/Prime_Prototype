package ai.flmg.unity;

import android.util.Log;

import org.json.JSONException;
import org.json.JSONObject;

import ai.flmg.plugin.CommonKt;
import ai.flmg.plugin.MapperUtilsKt;
import ai.flmg.plugin.FlamingoError;
import ai.flmg.plugin.OnError;

import com.unity3d.player.UnityPlayer;

public class FlamingoWrapper {
    private static final String METHOD_HANDLE_LOG = "_handleFlamingoLog";

    private static String gameObject = null;

    public static void setup(
        String gameObject,
        String accessKey,
        String appVersion
    ) {
        FlamingoWrapper.gameObject = gameObject;
        CommonKt.configure(
            UnityPlayer.currentActivity,
            accessKey,
            appVersion,
            new OnError() {
                @Override
                public void onError(FlamingoError error) {
                    sendError(error, METHOD_HANDLE_LOG);
                }
            }
        );

        sendFlamingoLogMessage("FlamingoWrapper.setup", METHOD_HANDLE_LOG);
    }

    public static void login(String appUserId) {
        CommonKt.login(
            appUserId,
            new OnError() {
                @Override
                public void onError(FlamingoError error) {
                    sendError(error, METHOD_HANDLE_LOG);
                }
            }
        );
        sendFlamingoLogMessage("FlamingoWrapper.login", METHOD_HANDLE_LOG);
    }

    public static void logEvent(
        String eventCode,
        String eventData,
        String customEventCode
    ) {
        CommonKt.logEvent(
            eventCode,
            eventData,
            customEventCode,
            new OnError() {
                @Override
                public void onError(FlamingoError error) {
                    sendError(error, METHOD_HANDLE_LOG);
                }
            }
        );
        sendFlamingoLogMessage("FlamingoWrapper.logEvent", METHOD_HANDLE_LOG);
    }

    public static void startPlaySession(String playSessionInfo, String customParams) {
        CommonKt.startPlaySession(
            playSessionInfo,
            customParams,
            new OnError() {
                @Override
                public void onError(FlamingoError error) {
                    sendError(error, METHOD_HANDLE_LOG);
                }
            }
        );
        sendFlamingoLogMessage("FlamingoWrapper.startPlaySession", METHOD_HANDLE_LOG);
    }

    public static void endPlaySession(String result, String customParams) {
        CommonKt.endPlaySession(
            result,
            customParams,
            new OnError() {
                @Override
                public void onError(FlamingoError error) {
                    sendError(error, METHOD_HANDLE_LOG);
                }
            }
        );
        sendFlamingoLogMessage("FlamingoWrapper.endPlaySession", METHOD_HANDLE_LOG);
    }

    public static void finishTutorial(String tutorialId, String tutorialName, String customParams) {
        CommonKt.finishTutorial(
            tutorialId,
            tutorialName,
            customParams,
            new OnError() {
                @Override
                public void onError(FlamingoError error) {
                    sendError(error, METHOD_HANDLE_LOG);
                }
            }
        );
        sendFlamingoLogMessage("FlamingoWrapper.finishTutorial", METHOD_HANDLE_LOG);
    }

    public static void purchaseIAP(String store, String isoCurrency, double price, String transactionId, String itemId, String itemName, boolean isTest, String customParams) {
        CommonKt.purchaseIAP(
            store,
            isoCurrency,
            price,
            transactionId,
            itemId,
            itemName,
            isTest,
            customParams,
            new OnError() {
                @Override
                public void onError(FlamingoError error) {
                    sendError(error, METHOD_HANDLE_LOG);
                }
            }
        );
        sendFlamingoLogMessage("FlamingoWrapper.purchaseIAP", METHOD_HANDLE_LOG);
    }

    public static void changeAssetAmount(
        String assetId,
        String assetName,
        long amountDiff,
        long resultAmount,
        String actionId,
        String actionName,
        String objectId,
        String objectName,
        String customParams
    ) {
        CommonKt.changeAssetAmount(
            assetId,
            assetName,
            amountDiff,
            resultAmount,
            actionId,
            actionName,
            objectId,
            objectName,
            customParams,
            new OnError() {
                @Override
                public void onError(FlamingoError error) {
                    sendError(error, METHOD_HANDLE_LOG);
                }
            }
        );
        sendFlamingoLogMessage("FlamingoWrapper.changeAssetAmount", METHOD_HANDLE_LOG);
    }

    public static void changeItemAmount(
        String itemId,
        String itemName,
        long amountDiff,
        long resultAmount,
        String actionId,
        String actionName,
        String objectId,
        String objectName,
        String customParams
    ) {
        CommonKt.changeItemAmount(
            itemId,
            itemName,
            amountDiff,
            resultAmount,
            actionId,
            actionName,
            objectId,
            objectName,
            customParams,
            new OnError() {
                @Override
                public void onError(FlamingoError error) {
                    sendError(error, METHOD_HANDLE_LOG);
                }
            }
        );
        sendFlamingoLogMessage("FlamingoWrapper.changeItemAmount", METHOD_HANDLE_LOG);
    }

    static void sendError(FlamingoError error, String method) {
        JSONObject json = new JSONObject();
        try {
            json.put("message", error.getMessage());
            json.put("error", MapperUtilsKt.convertToJson(error.getInfo()));
        } catch (JSONException e) {
            Log.e("Flamingo", "JSON error: " + e.getLocalizedMessage());
        }

        sendFlamingoLogMessage(json.toString(), method);
    }

    static void sendFlamingoLogMessage(String message, String method) {
        UnityPlayer.UnitySendMessage(FlamingoWrapper.gameObject, method, message);
    }
}