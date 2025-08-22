pipeline {
  agent { label 'mac-blob' }
//  agent { label 'mac-unity-agent' }
//   agent { label 'mac-corgi' }
//
// 프로젝트별로 environment 변수들을 수정해서 사용
  environment {
    UNITY_VERSION = "2022.3.53"
    ALOHA_ARTIFACT_GCS_ROOT_PATH = "team-blob/ddd/builds"
    GCP_CRED_ID = "aloha-infra-jenkins-sa-key"
    ANDROID_KEYSTORE_CRED_ID = "ddd-keystore"
    ANDROID_KEYSTORE_CRED_PW_ID = "ddd-keystore-password"
    KEYSTORE_FILE_NAME = "ddd.keystore"
    KEYSTORE_ALIAS_NAME = "ddd"
    PROJECT_CODE = "DDD"
  }
  parameters {
      booleanParam(name: 'IS_DEBUG', defaultValue: true, description: '디버그 빌드')
      booleanParam(name: 'USE_DEV_SERVER', defaultValue: true, description: '개발 서버 사용')
      booleanParam(name: 'CLEAR_CACHE', defaultValue: false, description: '빌드 전 캐시 클리어')
      booleanParam(name: 'BUILD_IOS', defaultValue: true, description: 'iOS 빌드')
      booleanParam(name: 'BUILD_APK', defaultValue: true, description: '안드로이드 프로젝트에서 APK 생성')
      booleanParam(name: 'BUILD_AAB', defaultValue: false, description: '안드로이드 프로젝트에서 AAB 생성')
      booleanParam(name: 'ONESTORE' , defaultValue: false, description: '원스토어 빌드')
  }
  triggers {
    githubPush()
  }
  stages {
    stage('Find Unity') {
      steps {
        script {
          def unityHubEditorPath = sh(script: "ls -d /Applications/Unity/Hub/Editor/*${UNITY_VERSION}*", returnStdout: true).trim()
          if (unityHubEditorPath.isEmpty()) {
            error "Unity ${UNITY_VERSION} not found."
          }

          env.UNITY_EDITOR_PATH = unityHubEditorPath + "/Unity.app/Contents/MacOS/Unity"
          env.ARGS = "-quit -batchmode -nographics -projectPath ${env.WORKSPACE.replaceAll(' ', '\\\\ ')} -isDebug=${params.IS_DEBUG} -clearCache=${params.CLEAR_CACHE} -useDevServer=${params.USE_DEV_SERVER}"
        }
      }
    }

    stage ('Build iOS') {
      when {
        expression { params.BUILD_IOS }
      }
      steps {
        script {
          sh """
          ${env.UNITY_EDITOR_PATH} ${env.ARGS} -buildTarget iOS -executeMethod JenkinsBuildScript.Build_iOS
          """
        }
      }
    }
    
    stage ('Build XCode') {
      when {
        expression { params.BUILD_IOS }
      }
      steps {
        withCredentials([string(credentialsId: 'keychain-pwd', variable: 'KEYCHAIN_PWD')]) {
          script {
            sh "security unlock-keychain -p ${KEYCHAIN_PWD} ~/Library/Keychains/login.keychain-db"
          }
        }
        xcodeBuild(
          xcodeWorkspaceFile: "Builds/${PROJECT_CODE}_iOS/Unity-iPhone",
          xcodeSchema: "Unity-iPhone",
          ipaExportMethod: 'app-store',
          generateArchive: true,
          buildIpa: true,
          developmentTeamID: '63HMA37HGL',
          configuration: 'Release',
          assetPacksInBundle: true,
          stripSwiftSymbols: true,
          signingMethod: 'automatic',
          cleanBeforeBuild: false
        )
      }
    }
    
    stage ('Upload to TestFlight') {
      when {
        expression { params.BUILD_IOS }
      }
      steps {
        script {
          withCredentials([usernamePassword(credentialsId: 'appstore-connor', usernameVariable: 'APPSTORE_USERNAME', passwordVariable: 'APPSTORE_PASSWORD')]) {
            def productName = sh(script: "cat Builds/product_name", returnStdout: true).trim()
            sh "xcrun altool --upload-app --type ios --file build/Release-iphoneos/${productName}.ipa --username ${APPSTORE_USERNAME} --password ${APPSTORE_PASSWORD}" 
          }
        }
      }
    }
    
    stage('Build APK') {
      when {
        expression { params.BUILD_APK }
      }
      steps {
        script {
          withCredentials([file(credentialsId: env.ANDROID_KEYSTORE_CRED_ID, variable: 'ANDROID_KEYSTORE_CRED_FILE'), string(credentialsId: env.ANDROID_KEYSTORE_CRED_PW_ID, variable: 'ANDROID_KEYSTORE_PASSWORD')]) {
            sh """
            mkdir -p Builds
            cp ${ANDROID_KEYSTORE_CRED_FILE.replaceAll(' ', '\\\\ ')} Builds/${env.KEYSTORE_FILE_NAME}
            ${env.UNITY_EDITOR_PATH} ${env.ARGS} \
            -buildTarget Android -executeMethod JenkinsBuildScript.Build_Android -keystorePassword=${ANDROID_KEYSTORE_PASSWORD} \
            -isOneStore=${params.ONESTORE}
            """
            def version = sh(script: "cat Builds/version", returnStdout: true).trim()
            sh """
            mv Builds/${env.PROJECT_CODE}.apk Builds/${env.PROJECT_CODE}${version}.apk
            rm -f Builds/${KEYSTORE_FILE_NAME}
            """
            env.APK_FILE = "${PROJECT_CODE}${version}.apk"
          }
        }
      }
    }
    
    stage('Build AAB') {
      when {
        expression { params.BUILD_AAB }
      }
      steps {
        script {
          withCredentials([file(credentialsId: env.ANDROID_KEYSTORE_CRED_ID, variable: 'ANDROID_KEYSTORE_CRED_FILE'), string(credentialsId: env.ANDROID_KEYSTORE_CRED_PW_ID, variable: 'ANDROID_KEYSTORE_PASSWORD')]) {
            sh """
            mkdir -p Builds
            cp ${ANDROID_KEYSTORE_CRED_FILE.replaceAll(' ', '\\\\ ')} Builds/${env.KEYSTORE_FILE_NAME}
            ${env.UNITY_EDITOR_PATH} ${env.ARGS} \
            -buildTarget Android -executeMethod JenkinsBuildScript.Build_Android -keystorePassword=${ANDROID_KEYSTORE_PASSWORD} \
            -aab
            """
            def version = sh(script: "cat Builds/version", returnStdout: true).trim()
            sh """
            mv Builds/${env.PROJECT_CODE}.aab Builds/${env.PROJECT_CODE}${version}.aab
            rm -f Builds/${KEYSTORE_FILE_NAME}
            """
            //firebase crashlytics:symbols:upload --app=1:552398277580:android:a593a90a465e36b0faeb69 ./unityLibrary/symbols
            env.AAB_FILE = "${PROJECT_CODE}${version}.aab"
          }
        }
      }  
    }

    stage ('Archive artifacts') {
      steps {
        script {
          sh """
          cd ${env.WORKSPACE.replaceAll(' ', '\\\\ ')}
          """
          if (params.BUILD_APK) {
            archiveArtifacts artifacts: "Builds/${env.APK_FILE}", fingerprint: true
            stash(name: 'archived-artifacts-apk', includes: "Builds/${env.APK_FILE}")
          }
          if (params.BUILD_AAB) {
            archiveArtifacts artifacts: "Builds/${env.AAB_FILE}", fingerprint: true
            stash(name: 'archived-artifacts-aab', includes: "Builds/${env.AAB_FILE}")
          }
        }
      }
    }

//     stage ('Upload artifacts') {
//       agent { label 'ubuntu-2004' }
//       options { skipDefaultCheckout() }
//       steps {
//         script {
//           if(params.BUILD_APK) {
//             unstash 'archived-artifacts-apk'
//             withCredentials([file(credentialsId: env.GCP_CRED_ID, variable: 'GCP_CRED_FILE')]) {
//               sh "gcloud auth activate-service-account --key-file=${GCP_CRED_FILE.replaceAll(' ', '\\\\ ')}"
//               sh "gsutil cp Builds/${env.APK_FILE} gs://${env.ALOHA_ARTIFACT_GCS_BUCKET}/${env.ALOHA_ARTIFACT_GCS_ROOT_PATH}/${env.BUILD_NUMBER}/${env.APK_FILE}"
//             }
//           }
//           if(params.BUILD_AAB) {
//             unstash 'archived-artifacts-aab'
//             withCredentials([file(credentialsId: env.GCP_CRED_ID, variable: 'GCP_CRED_FILE')]) {
//               sh "gcloud auth activate-service-account --key-file=${GCP_CRED_FILE.replaceAll(' ', '\\\\ ')}"
//               sh "gsutil cp Builds/${env.AAB_FILE} gs://${env.ALOHA_ARTIFACT_GCS_BUCKET}/${env.ALOHA_ARTIFACT_GCS_ROOT_PATH}/${env.BUILD_NUMBER}/${env.AAB_FILE}"
//             }
//           }
//           if(params.BUILD_IOS) {
//             //echo no artifacts
//             sh "echo 'no artifacts for iOS'"
//           }
//         }
//       }
//     }
  }

  post {
    always {
      script {
        sh """
        rm -f Builds/${KEYSTORE_FILE_NAME}
        rm -f Builds/*.apk
        rm -f Builds/*.aab
        """
      }
    }
    success {
      script {
        def payload = """
        {
          "message": "🟢 ${env.JOB_NAME} has been successfully built.",
          "url": "${env.BUILD_URL}",
          "note": "${env.GIT_URL} ${env.GIT_COMMIT}"
        }
        """.trim()
        sh "curl -X POST -H 'Content-type: application/json' --data '${payload.replaceAll("'", "\\'")}' ${env.ALOHA_SLACK_WEBHOOK_URL}"
      }
    }
    unstable {
      script {
        def payload = """
        {
          "message": "🟡 ${env.JOB_NAME} has been successfully built, but some tests failed.",
          "url": "${env.BUILD_URL}",
          "note": "${env.GIT_URL} ${env.GIT_COMMIT}"
        }
        """.trim()
        sh "curl -X POST -H 'Content-type: application/json' --data '${payload.replaceAll("'", "\\'")}' ${env.ALOHA_SLACK_WEBHOOK_URL}"
      }
    }
    failure {
      script {
        def payload = """
        {
          "message": "🔴 ${env.JOB_NAME} has been failed.",
          "url": "${env.BUILD_URL}",
          "note": "${env.GIT_URL} ${env.GIT_COMMIT}"
        }
        """.trim()
        sh "curl -X POST -H 'Content-type: application/json' --data '${payload.replaceAll("'", "\\'")}' ${env.ALOHA_SLACK_WEBHOOK_URL}"
      }
    }
  }
}