package com.autofnhk.android

import android.accessibilityservice.AccessibilityService
import android.accessibilityservice.GestureDescription
import android.graphics.Path
import android.os.Handler
import android.os.Looper
import android.view.accessibility.AccessibilityEvent
import java.util.Collections

class AutoFnhkAccessibilityService : AccessibilityService() {
    companion object {
        @Volatile var instance: AutoFnhkAccessibilityService? = null
        val actionLog: MutableList<String> =
            Collections.synchronizedList(mutableListOf())
    }

    private val handler = Handler(Looper.getMainLooper())
    private var currentPackage: String? = null

    override fun onServiceConnected() {
        instance = this
        log("Controller connected")
    }

    override fun onAccessibilityEvent(event: AccessibilityEvent?) {
        currentPackage = event?.packageName?.toString() ?: currentPackage
    }

    override fun onInterrupt() {
        log("Controller interrupted")
    }

    override fun onDestroy() {
        instance = null
        log("Controller disconnected")
        super.onDestroy()
    }

    fun executeTestPlan(targetPackage: String, prompt: String, finished: () -> Unit) {
        if (currentPackage != targetPackage) {
            log("Target not foreground: expected " + targetPackage + ", found " + (currentPackage ?: "none"))
            finished()
            return
        }

        log("Plan: " + prompt.ifBlank { "aim movement test" })
        log("Input mode: Android touch gestures")

        val actions = listOf("aim right", "aim left", "aim up", "aim down")

        var index = 0
        fun next() {
            if (index >= actions.size || currentPackage != targetPackage) {
                log(if (currentPackage == targetPackage) "Plan complete" else "Plan stopped: target left foreground")
                finished()
                return
            }

            when (index) {
                0 -> aimSwipe(0.72f, 0.50f, 0.88f, 0.50f)
                1 -> aimSwipe(0.88f, 0.50f, 0.72f, 0.50f)
                2 -> aimSwipe(0.80f, 0.38f, 0.80f, 0.22f)
                3 -> aimSwipe(0.80f, 0.22f, 0.80f, 0.38f)
            }

            log("Action " + (index + 1) + "/" + actions.size + ": " + actions[index])
            index++
            handler.postDelayed(::next, 550)
        }

        next()
    }

    private fun aimSwipe(fromX: Float, fromY: Float, toX: Float, toY: Float) {
        val dm = resources.displayMetrics
        val path = Path().apply {
            moveTo(dm.widthPixels * fromX, dm.heightPixels * fromY)
            lineTo(dm.widthPixels * toX, dm.heightPixels * toY)
        }

        val stroke = GestureDescription.StrokeDescription(path, 0, 300)
        val gesture = GestureDescription.Builder().addStroke(stroke).build()
        dispatchGesture(gesture, null, null)
    }

    private fun log(message: String) {
        actionLog.add(message)
        while (actionLog.size > 100) actionLog.removeAt(0)
    }
}
