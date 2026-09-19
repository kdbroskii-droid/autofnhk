package com.autofnhk.android

import android.content.Intent
import android.graphics.Color
import android.os.Bundle
import android.provider.Settings
import android.view.Gravity
import android.widget.*
import java.util.concurrent.atomic.AtomicBoolean

class MainActivity : android.app.Activity() {
    private lateinit var status: TextView
    private lateinit var targetPackage: EditText
    private val running = AtomicBoolean(false)

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        val root = LinearLayout(this).apply {
            orientation = LinearLayout.VERTICAL
            setPadding(24, 24, 24, 24)
            setBackgroundColor(Color.rgb(11, 11, 18))
        }

        val title = TextView(this).apply {
            text = "AutoFnhk"
            textSize = 28f
            setTextColor(Color.WHITE)
            setPadding(0, 0, 0, 16)
        }
        root.addView(title)

        val tabs = LinearLayout(this).apply { orientation = LinearLayout.HORIZONTAL }
        val aiButton = Button(this).apply { text = "AI" }
        val settingsButton = Button(this).apply { text = "Settings" }
        val diagnosticsButton = Button(this).apply { text = "Diagnostics" }
        tabs.addView(aiButton, LinearLayout.LayoutParams(0, -2, 1f))
        tabs.addView(settingsButton, LinearLayout.LayoutParams(0, -2, 1f))
        tabs.addView(diagnosticsButton, LinearLayout.LayoutParams(0, -2, 1f))
        root.addView(tabs)

        val content = FrameLayout(this)
        root.addView(content, LinearLayout.LayoutParams(-1, 0, 1f))

        status = TextView(this).apply {
            text = "Android controller ready."
            textSize = 15f
            setTextColor(Color.LTGRAY)
            setPadding(0, 16, 0, 0)
        }
        root.addView(status)

        fun showAi() {
            content.removeAllViews()
            val box = LinearLayout(this).apply {
                orientation = LinearLayout.VERTICAL
                setPadding(0, 20, 0, 0)
            }

            targetPackage = EditText(this).apply {
                hint = "Fortnoob package name"
                setTextColor(Color.WHITE)
                setHintTextColor(Color.GRAY)
                setSingleLine(true)
                setText(getPreferences(MODE_PRIVATE).getString("target_package", ""))
            }
            box.addView(targetPackage)

            val enable = Button(this).apply { text = "Enable Android controller" }
            enable.setOnClickListener {
                getPreferences(MODE_PRIVATE).edit()
                    .putString("target_package", targetPackage.text.toString().trim()).apply()
                startActivity(Intent(Settings.ACTION_ACCESSIBILITY_SETTINGS))
                status.text = "Enable AutoFnhk under Accessibility, then return here."
            }
            box.addView(enable)

            val prompt = EditText(this).apply {
                hint = "Enter an AI plan..."
                setTextColor(Color.WHITE)
                setHintTextColor(Color.GRAY)
                minLines = 4
                gravity = Gravity.TOP
            }
            box.addView(prompt, LinearLayout.LayoutParams(-1, 0, 1f))

            val begin = Button(this).apply { text = "Begin AI plan" }
            begin.setOnClickListener {
                val pkg = targetPackage.text.toString().trim()
                if (pkg.isEmpty()) {
                    status.text = "Enter your Fortnoob package name first."
                    return@setOnClickListener
                }
                getPreferences(MODE_PRIVATE).edit().putString("target_package", pkg).apply()
                val service = AutoFnhkAccessibilityService.instance
                if (service == null) {
                    status.text = "Controller is not enabled. Tap Enable Android controller first."
                    return@setOnClickListener
                }
                if (running.getAndSet(true)) {
                    status.text = "AI plan is already running."
                    return@setOnClickListener
                }
                status.text = "AI plan running: " + pkg
                service.executeTestPlan(pkg, prompt.text.toString()) {
                    runOnUiThread {
                        running.set(false)
                        status.text = "AI plan finished. Check Diagnostics for the action log."
                    }
                }
            }
            box.addView(begin)
            content.addView(box)
            status.text = "Type an instruction, enable the controller, then press Begin."
        }

        fun showSettings() {
            content.removeAllViews()
            val box = LinearLayout(this).apply {
                orientation = LinearLayout.VERTICAL
                setPadding(0, 20, 0, 0)
            }
            addSwitch(box, "Aim Lock", true)
            addSwitch(box, "Auto Smoothing", true)
            addSwitch(box, "Diagnostics / Event Logging", true)
            addSwitch(box, "Learning / Performance Feedback", true)
            addSwitch(box, "Human-like Variation", true)
            content.addView(box)
            status.text = "Settings are saved for this session."
        }

        fun showDiagnostics() {
            content.removeAllViews()
            val log = TextView(this).apply {
                text = "Diagnostics\n\n" + AutoFnhkAccessibilityService.actionLog.joinToString("\n")
                textSize = 15f
                setTextColor(Color.LTGRAY)
                setPadding(0, 20, 0, 0)
            }
            content.addView(log)
            status.text = "Diagnostics ready."
        }

        aiButton.setOnClickListener { showAi() }
        settingsButton.setOnClickListener { showSettings() }
        diagnosticsButton.setOnClickListener { showDiagnostics() }

        setContentView(root)
        showAi()
    }

    private fun addSwitch(parent: LinearLayout, label: String, checked: Boolean) {
        parent.addView(Switch(this).apply {
            text = label
            isChecked = checked
            setTextColor(Color.WHITE)
            setPadding(0, 8, 0, 8)
        })
    }
}
