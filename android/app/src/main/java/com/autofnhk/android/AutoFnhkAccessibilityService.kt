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

        log("Plan: " + prompt.ifBlank { "basic controller test" })
        val actions = listOf("tap center", "swipe left", "swipe right", "tap center")

        var index = 0
        fun next() {
            if (index >= actions.size || currentPackage != targetPackage) {
                log(if (currentPackage == targetPackage) "Plan complete" else "Plan stopped: target left foreground")
                finished()
                return
            }

            when (index) {
                0, 3 -> tapCenter()
                1 -> swipe(0.25f, 0.50f, 0.75f, 0.50f)
                2 -> swipe(0.75f, 0.50f, 0.25f, 0.50f)
            }
            log("Action " + (index + 1) + "/" + actions.size + ": " + actions[index])
            index++
            handler.postDelayed(::next, 450)
        }
        next()
    }

    private fun tapCenter() {
        val dm = resources.displayMetrics
        val path = Path().apply { moveTo(dm.widthPixels / 2f, dm.heightPixels / 2f) }
        val stroke = GestureDescription.StrokeDescription(path, 0, 80)
        dispatchGesture(GestureDescription.Builder().addStroke(stroke).build(), null, null)
    }

    private fun swipe(fromX: Float, fromY: Float, toX: Float, toY: Float) {
        val dm = resources.displayMetrics
        val path = Path().apply {
            moveTo(dm.widthPixels * fromX, dm.heightPixels * fromY)
            lineTo(dm.widthPixels * toX, dm.heightPixels * toY)
        }
        val stroke = GestureDescription.StrokeDescription(path, 0, 250)
        dispatchGesture(GestureDescription.Builder().addStroke(stroke).build(), null, null)
    }

    private fun log(message: String) {
        actionLog.add(message)
        while (actionLog.size > 100) actionLog.removeAt(0)
    }
}
