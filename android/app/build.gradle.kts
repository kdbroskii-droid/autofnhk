plugins {
    id("com.android.application")
    id("org.jetbrains.kotlin.android")
}

android {
    namespace = "com.autofnhk.android"
    compileSdk = 35

    defaultConfig {
        applicationId = "com.autofnhk.android"
        minSdk = 29
        targetSdk = 35
        versionCode = 1
        versionName = "1.0"
    }
}

kotlin {
    jvmToolchain(17)
}
